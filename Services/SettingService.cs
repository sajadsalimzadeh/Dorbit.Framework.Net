using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
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

    public Setting Get(string key)
    {
        Load();
        return Settings.GetValueOrDefault(key);
    }

    public T Get<T>(string key, T defaultValue)
    {
        var setting = Get(key);
        if (setting is null)
        {
            setting = new Setting()
            {
                Key = key,
            };
            setting.SetValue(defaultValue);
            try
            {
                settingRepository.InsertAsync(setting).Wait();
            }
            catch
            {
                // ignored
            }

            Settings[setting.Key] = setting;
        }

        return setting.GetValue(defaultValue);
    }

    public Setting Get(Enum key)
    {
        return Get(key.ToString());
    }

    public T Get<T>(Enum key, T defaultValue)
    {
        return Get(key.ToString(), defaultValue);
    }

    public List<Setting> GetAll()
    {
        Load();
        return Settings.Values.ToList();
    }

    public async Task SaveAsync(string key, dynamic value)
    {
        _isChangeDetected = true;
        var setting = await settingRepository.Set().FirstOrDefaultAsync(x => x.Key == key);
        if (setting is null)
        {
            setting = new Setting()
            {
                Key = key,
                Value = value.ToString()
            };
            await settingRepository.InsertAsync(setting);
        }
    }

    public async Task SaveAllAsync(Dictionary<string, dynamic> dict)
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
                existsSetting.Value = keyValue.Value.ToString();
                updateEntities.Add(existsSetting);
            }
            else
            {
                insertEntities.Add(new Setting()
                {
                    Key = keyValue.Key,
                    Value = keyValue.Value.ToString()
                });
            }
        }

        await settingRepository.BulkInsertAsync(insertEntities);
        await settingRepository.BulkUpdateAsync(updateEntities);
    }
}