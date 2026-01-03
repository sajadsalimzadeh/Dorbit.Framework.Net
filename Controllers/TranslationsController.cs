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
    [Auth, HttpPost]
    public Task<QueryResult<List<Translation>>> TranslateAllAsync([FromQuery] string locale, [FromBody] List<string> keys)
    {
        if(keys == null || keys.Count == 0) throw  new ArgumentNullException(nameof(keys));
        var query = translationRepository.Set();
        if (locale.IsNotNullOrEmpty()) query = query.Where(x => x.Locale == locale);
        return query.Where(x => keys.Contains(x.Key)).ToListAsync().ToQueryResultAsync();
    }
}