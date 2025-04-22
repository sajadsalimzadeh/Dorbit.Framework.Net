using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Repositories;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class SettingService(SettingRepository settingRepository, IAuthService authService)
{

    public async Task<string> GetAsync(string key)
    {
        var accesses = await authService.GetAllAccessAsync();
        var setting = await settingRepository.Set().FirstOrDefaultAsync(x => x.Key == key);
        if (setting is null || setting.Value.IsNullOrEmpty()) return default;
        try
        {
            if (setting.Access.IsNotNullOrEmpty() && !accesses.Contains(setting.Access)) return default;
            return setting.Value;
        }
        catch (Exception e)
        {
            return default;
        }
    }

    public async Task<Dictionary<string, string>> GetAllAsync(List<string> keys)
    {
        var accesses = await authService.GetAllAccessAsync();
        var query = settingRepository.Set().AsQueryable();
        if (keys is not null && keys.Count > 0) query = query.Where(x => keys.Contains(x.Key));
        var settings = await query.ToListAsync();
        settings = settings.Where(x => x.Access.IsNullOrEmpty() || accesses.Contains(x.Access)).ToList();
        return settings.ToDictionary(setting => setting.Key, setting => setting.Value);
    }

    public async Task SaveAsync(string key, dynamic value)
    {
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