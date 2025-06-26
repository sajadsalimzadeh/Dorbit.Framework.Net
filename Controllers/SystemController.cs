using System.Collections.Generic;
using System.Linq;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Dorbit.Framework.Controllers;

[Route("Framework/[controller]")]
public class SystemController(IMemoryCache memoryCache) : BaseController
{
    [HttpGet("MemoryCache")]
    public QueryResult<List<object>> GetAllMemoryCacheKeys()
    {
        if (memoryCache is MemoryCache mc)
        {
            return mc.Keys.ToList().ToQueryResult();
        }

        return new List<object>().ToQueryResult();
    }
    
    [HttpDelete("MemoryCache")]
    public CommandResult DeleteMemoryCache()
    {
        if (memoryCache is MemoryCache mc)
        {
            mc.Clear();
            return CommandResult.Succeed();
        }

        return CommandResult.Failed("Memory cache instance not supported");
    }
    
    [HttpDelete("MemoryCache/{key}")]
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