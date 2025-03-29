using System.Collections.Generic;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Repositories;
using Dorbit.Framework.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dorbit.Framework.Controllers;

public class SettingsController(SettingService settingService) : BaseController
{

    [HttpGet]
    public Task<QueryResult<Dictionary<string, string>>> GetAllAsync([FromQuery] List<string> keys)
    {
        return settingService.GetAllAsync(keys).ToQueryResultAsync();
    }
    
    [HttpGet("{key}")]
    public Task<QueryResult<string>> GetAsync(string key)
    {
        return settingService.GetAsync(key).ToQueryResultAsync();
    }
    
    [HttpPost, Auth("Setting")]
    public async Task<CommandResult> SaveAllAsync([FromBody] Dictionary<string, dynamic> dict)
    {
        await settingService.SaveAllAsync(dict);
        return Succeed();
    }
}