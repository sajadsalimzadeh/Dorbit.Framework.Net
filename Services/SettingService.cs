using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Abstractions;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Entities.Abstractions;
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

    public T Get<T>(string key, T defaultValue = default)
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

    public List<Setting> GetAll()
    {
        return Settings.Values.ToList();
    }


    public async Task SetAsync<T>(T value) where T : class, ISettingDto
    {
        Validate(value);
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

    public async Task SaveAllAsync(Dictionary<string, JsonElement> dict)
    {
        var keys = dict.Keys;
        var settings = await settingRepository.Set().Where(x => keys.Contains(x.Key)).ToListAsync();
        var insertEntities = new List<Setting>();
        var updateEntities = new List<Setting>();
        foreach (var keyValue in dict)
        {
            var settingType = SettingTypes.GetValueOrDefault(keyValue.Key);
            if(settingType is null) return;
            
            var settingDto = Activator.CreateInstance(settingType);
            settingDto = settingDto.PatchObjectWithJson(keyValue.Value, settingType, settingType);
            Validate(settingDto);
            
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

    private static void Validate(object value)
    {
        if (value is not IValidate validator) return;
        var error = new ModelValidationException();
        validator.Validate(error);
        error.ThrowIfHasError();
    }
}
