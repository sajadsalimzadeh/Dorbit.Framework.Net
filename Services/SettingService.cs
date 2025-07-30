using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Abstractions;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class SettingService(SettingRepository settingRepository)
{
    private static bool _isChangeDetected = true;
    private static readonly Dictionary<string, Setting> Settings = new();

    private void Load()
    {
        if (!_isChangeDetected) return;
        var allSettings = settingRepository.Set().ToList();
        foreach (var setting in allSettings)
        {
            Settings[setting.Key] = setting;
        }
    }

    public T Get<T>(T defaultValue = null) where T : class, ISettingDto
    {
        var instance = Activator.CreateInstance<T>();
        return Get(instance.GetKey(), defaultValue ?? instance);
    }
    
    public T Get<T>(string key, T defaultValue = default)
    {
        Load();
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
        Load();
        return Settings.Values.ToList();
    }


    public async Task SaveAsync<T>(T value) where T : class, ISettingDto
    {
        var key = value.GetKey();
        _isChangeDetected = true;
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
    }

    public async Task SaveAllAsync(Dictionary<string, object> dict)
    {
        var keys = dict.Keys;
        var settings = await settingRepository.Set().Where(x => keys.Contains(x.Key)).ToListAsync();
        var insertEntities = new List<Setting>();
        var updateEntities = new List<Setting>();
        foreach (var keyValue in dict)
        {
            var existsSetting = settings.FirstOrDefault(x => x.Key == keyValue.Key);
            if (existsSetting is not null)
            {
                existsSetting.SetValue(keyValue.Value);
                updateEntities.Add(existsSetting);
            }
            else
            {
                insertEntities.Add(new Setting(keyValue.Key, keyValue.Value));
            }
        }

        await settingRepository.BulkInsertAsync(insertEntities);
        await settingRepository.BulkUpdateAsync(updateEntities);
    }
}