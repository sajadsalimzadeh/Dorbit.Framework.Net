using System.Collections.Generic;
using System.Linq;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Dorbit.Framework.Controllers;

[Route("Framework/[controller]")]
public class SystemController(IMemoryCache memoryCache) : BaseController
{
    [HttpGet("MemoryCache/Keys"), Auth("System-MemoryCache")]
    public QueryResult<List<string>> GetAllMemoryCacheKeys()
    {
        if (memoryCache is MemoryCache mc)
        {
            return mc.Keys.Select(x => x.ToString()).ToList().ToQueryResult();
        }

        return new List<string>().ToQueryResult();
    }
    
    [HttpPost("MemoryCache/Clear"), Auth("System-MemoryCache")]
    public CommandResult DeleteMemoryCache()
    {
        if (memoryCache is MemoryCache mc)
        {
            mc.Clear();
            return CommandResult.Succeed();
        }

        return CommandResult.Failed("Memory cache instance not supported");
    }
    
    [HttpPost("MemoryCache/Delete/{key}"), Auth("System-MemoryCache")]
    public CommandResult DeleteMemoryCache([FromRoute] string key)
    {
        if (memoryCache is MemoryCache mc)
        {
            mc.Remove(key);
            return CommandResult.Succeed();
        }

        return CommandResult.Failed("Memory cache instance not supported");
    }
}