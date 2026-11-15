using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Abstractions;
using Dorbit.Framework.Contracts.Identities;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class SettingService(SettingRepository settingRepository)
{
    private static readonly ConcurrentDictionary<string, Setting> Settings = new();
    private static readonly ConcurrentDictionary<string, Type> SettingTypes = new();

    public async Task InitAsync()
    {
        var allSettings = await settingRepository.Set().ToListAsync();
        foreach (var setting in allSettings)
        {
            Settings[setting.Key] = setting;
        }

        var settingDtoType = typeof(ISettingDto);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var types = assembly.GetTypes().Where(x => x.IsClass && x.IsAssignableTo(settingDtoType)).ToList();
            foreach (var settingType in types)
            {
                if (Activator.CreateInstance(settingType) is ISettingDto settingDto)
                {
                    SettingTypes.TryAdd(settingDto.GetKey(), settingType);
                }
            }
        }
    }

    public T Get<T>(T defaultValue = null) where T : class, ISettingDto
    {
        var instance = Activator.CreateInstance<T>();
        return Get(instance.GetKey(), defaultValue ?? instance);
    }

    public T Get<T>(string key, T defaultValue) where T : class, ISettingDto
    {
        var setting = Settings.GetValueOrDefault(key);
        if (setting is not null) return setting.GetValue(defaultValue);

        setting = new Setting(key, defaultValue);
        try
        {
            settingRepository.InsertAsync(setting).Wait();
        }
        catch
        {
            // ignored
        }

        Settings[setting.Key] = setting;

        return setting.GetValue(defaultValue);
    }

    public JsonElement? Get(string key)
    {
        var setting = Settings.GetValueOrDefault(key);
        if (setting is not null && setting.Value.IsNotNullOrEmpty()) return JsonSerializer.Deserialize<JsonElement>(setting.Value);
        return null;
    }

    public List<Setting> GetAll()
    {
        return Settings.Values.ToList();
    }


    public async Task SetAsync<T>(T value) where T : class, ISettingDto
    {
        var key = value.GetKey();
        var setting = await settingRepository.Set().FirstOrDefaultAsync(x => x.Key == key);
        if (setting is null)
        {
            setting = new Setting(key, value);
            await settingRepository.InsertAsync(setting);
        }
        else
        {
            setting.SetValue(value);
            await settingRepository.UpdateAsync(setting);
        }

        Settings[setting.Key] = setting;
    }

    public async Task SaveAllAsync(IdentityDto identity, Dictionary<string, JsonElement> dict)
    {
        var keys = dict.Keys;
        var settings = await settingRepository.Set().Where(x => keys.Contains(x.Key)).ToListAsync();
        var insertEntities = new List<Setting>();
        var updateEntities = new List<Setting>();
        foreach (var keyValue in dict)
        {
            var settingType = SettingTypes.GetValueOrDefault(keyValue.Key);
            if (settingType is null) throw new OperationException(FrameworkErrors.SettingKeyTypeNotFound);
            var patch = AuthorizeSetPropertiesAsync(identity, keyValue.Key, keyValue.Value);

            var settingDto = Activator.CreateInstance(settingType) as ISettingDto;
            settingDto = settingDto.PatchObjectWithJson(patch, settingType, settingType) as ISettingDto;

            var setting = settings.FirstOrDefault(x => x.Key == keyValue.Key);
            if (setting is not null)
            {
                setting.SetValue(settingDto);
                updateEntities.Add(setting);
            }
            else
            {
                insertEntities.Add(setting = new Setting(keyValue.Key, settingDto));
            }

            Settings[keyValue.Key] = setting;
        }

        await settingRepository.BulkInsertAsync(insertEntities);
        await settingRepository.BulkUpdateAsync(updateEntities);
    }

    public JsonElement AuthorizeSetPropertiesAsync(IdentityDto identity, string key, JsonElement json)
    {
        var type = SettingTypes.GetValueOrDefault(key);
        if (type is null) throw new OperationException(FrameworkErrors.SettingKeyTypeNotFound);
        
        var typeAuthSettingAttr = type.GetCustomAttribute<AuthSettingAttribute>();
        var properties = type.GetProperties();

        var jsonObject = JsonNode.Parse(json.GetRawText())!.AsObject();

        foreach (var propertyInfo in properties)
        {
            if (json.TryGetPropertyIgnoreCase(propertyInfo.Name, out var jsonProperty))
            {
                var authAttr = propertyInfo.SetMethod?.GetCustomAttribute<AuthSettingAttribute>() ?? propertyInfo.GetCustomAttribute<AuthSettingAttribute>() ?? typeAuthSettingAttr;
                if (authAttr is not null && authAttr.Access.IsNotNullOrEmpty())
                {
                    if (!identity.HasAccess(authAttr.Access))
                        jsonObject.Remove(jsonProperty.Name);
                }
            }
        }

        return JsonSerializer.Deserialize<JsonElement>(jsonObject.ToJsonString());
    }

    public JsonElement AuthorizeGetPropertiesAsync(IdentityDto identity, string key, JsonElement json)
    {
        var type = SettingTypes.GetValueOrDefault(key);
        if (type is null) throw new OperationException(FrameworkErrors.SettingKeyTypeNotFound);
        
        var typeAuthSettingAttr = type.GetCustomAttribute<AuthSettingAttribute>();
        var properties = type.GetProperties();

        var jsonObject = JsonNode.Parse(json.GetRawText())!.AsObject();

        foreach (var propertyInfo in properties)
        {
            if (json.TryGetPropertyIgnoreCase(propertyInfo.Name, out var jsonProperty))
            {
                var authAttr = propertyInfo.GetMethod?.GetCustomAttribute<AuthSettingAttribute>() ?? propertyInfo.GetCustomAttribute<AuthSettingAttribute>() ?? typeAuthSettingAttr;
                if (authAttr is not null && authAttr.Access.IsNotNullOrEmpty())
                {
                    if (!identity.HasAccess(authAttr.Access))
                        jsonObject.Remove(jsonProperty.Name);
                }
            }
        }

        return JsonSerializer.Deserialize<JsonElement>(jsonObject.ToJsonString());
    }
}