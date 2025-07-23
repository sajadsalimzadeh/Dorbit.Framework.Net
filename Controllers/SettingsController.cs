using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Contracts.Settings;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dorbit.Framework.Controllers;

[ApiExplorerSettings(GroupName = "framework")]
[Route("Framework/[controller]")]
public class SettingsController(SettingService settingService) : BaseController
{
    [HttpGet]
    public QueryResult<Dictionary<string, object>> GetAll([FromQuery] List<string> keys)
    {
        var settings = settingService.GetAll();
        if (keys is { Count: > 0 }) settings = settings.Where(x => keys.Contains(x.Key)).ToList();
        var result = new Dictionary<string, object>();
        foreach (var setting in settings)
        {
            result.Add(setting.Key.ToLower(), setting.GetValue<object>());
        }

        return result.ToQueryResult();
    }

    [HttpGet("{key}")]
    public QueryResult<object> Get(string key)
    {
        var value = settingService.Get<object>(key);
        return value.ToQueryResult();
    }

    [HttpPost, Auth("Setting")]
    public async Task<CommandResult> SaveAsync([FromBody] Dictionary<string, dynamic> dict)
    {
        await settingService.SaveAllAsync(dict);
        return Succeed();
    }
}