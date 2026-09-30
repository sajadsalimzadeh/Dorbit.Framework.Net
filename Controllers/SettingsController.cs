using System;
using System.Collections.Generic;
using System.Linq;
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
    [HttpGet, Auth("Setting")]
    public QueryResult<Dictionary<string, object>> GetAll([FromQuery] List<string> keys)
    {
        var settings = settingService.GetAll().Where(CanRead).ToList();
        if (keys is { Count: > 0 }) settings = settings.Where(x => keys.Contains(x.Key)).ToList();
        var result = new Dictionary<string, object>();
        foreach (var setting in settings)
        {
            result.Add(setting.Key.ToLower(), setting.GetValue<object>());
        }

        return result.ToQueryResult();
    }

    [HttpGet("{key}"), Auth("Setting")]
    public QueryResult<object> Get(string key)
    {
        var setting = settingService.GetAll()
            .FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
        if (setting is null)
            return default(object).ToQueryResult();
        if (!CanRead(setting))
            throw new UnauthorizedAccessException();

        return setting.GetValue<object>().ToQueryResult();
    }

    private bool CanRead(Setting setting)
    {
        if (setting.Access.IsNullOrEmpty())
            return true;

        var identity = Identity;
        return identity is not null && (identity.IsFullAccess || identity.HasAccess(setting.Access));
    }

    [HttpPost, Auth("Setting")]
    public async Task<CommandResult> SaveAsync([FromBody] Dictionary<string, dynamic> dict)
    {
        await settingService.SaveAllAsync(dict);
        return Succeed();
    }
}