using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Controllers;

[Route("Framework/[controller]")]
public class TranslationsController(TranslationRepository translationRepository) : BaseController
{
    [Auth, HttpPost("{locale}"), ResponseCache(Duration = 60)]
    public Task<QueryResult<List<Translation>>> TranslateAllAsync([FromRoute] string locale,
        [FromBody] List<string> keys)
    {
        var query = translationRepository.Set().Where(x => x.Locale == locale);
        if (keys.IsNotNullOrEmpty()) query = query.Where(x => keys.Contains(x.Key));
        return query.ToListAsync().ToQueryResultAsync();
    }
}