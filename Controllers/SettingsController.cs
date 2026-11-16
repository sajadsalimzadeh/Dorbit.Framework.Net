using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Results;
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
    [HttpGet, Auth]
    public QueryResult<Dictionary<string, JsonElement?>> GetAll([FromQuery] List<string> keys)
    {
        var result = new Dictionary<string, JsonElement?>();
        foreach (var key in keys)
        {
            var setting = settingService.Get(key);
            if(setting.HasValue)
            {
                settingService.ApplyAuthentication(key, setting.Value, Identity, false);
            }
            result.Add(key.ToLower(), setting);
        }

        return result.ToQueryResult();
    }

    [HttpGet("{key}"), Auth]
    public QueryResult<JsonElement?> Get(string key)
    {
        return settingService.Get(key).ToQueryResult();
    }

    [HttpPost, Auth("Setting")]
    public async Task<CommandResult> SaveAsync([FromBody] Dictionary<string, JsonElement> dict)
    {
        foreach (var key in dict.Keys)
        {
            dict[key] = settingService.ApplyAuthentication(key, dict[key.ToLower()], Identity, true);
        }

        await settingService.SaveAllAsync(dict);
        return Succeed();
    }
}