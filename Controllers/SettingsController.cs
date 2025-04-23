using System.Collections.Generic;
using System.Linq;
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
public class SettingsController(SettingService settingService) : BaseController
{

    [HttpGet]
    public QueryResult<List<SettingDto>> GetAll([FromQuery] List<string> keys)
    {
        var settings = settingService.GetAll();
        if(keys is { Count: > 0 }) settings = settings.Where(x => keys.Contains(x.Key)).ToList();
        return settings.MapTo<Setting, SettingDto>().ToQueryResult();
    }
    
    [HttpGet("{key}")]
    public QueryResult<SettingDto> Get(string key)
    {
        return settingService.Get(key).MapTo<SettingDto>().ToQueryResult();
    }
    
    [HttpPost, Auth("Setting")]
    public async Task<CommandResult> SaveAllAsync([FromBody] Dictionary<string, dynamic> dict)
    {
        await settingService.SaveAllAsync(dict);
        return Succeed();
    }
}