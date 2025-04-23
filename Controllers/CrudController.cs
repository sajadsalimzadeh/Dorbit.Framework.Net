using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Repositories.Abstractions;
using Dorbit.Framework.Utils.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Dorbit.Framework.Controllers;

public abstract class CrudController : BaseController;

public abstract class CrudController<TEntity, TKey, TGet, TAdd, TEdit> : CrudController where TEntity : class, IEntity<TKey>
{
    protected IBaseRepository<TEntity, TKey> Repository => ServiceProvider.GetService<IBaseRepository<TEntity, TKey>>();
    
    protected virtual IQueryable<TEntity> Set() => Repository.Set();

    [HttpGet]
    public virtual async Task<PagedListResult<TGet>> SelectAsync()
    {
        var query = Set();
        if (typeof(TEntity).IsAssignableTo(typeof(ICreationTime)))
        {
            query = query.Cast<ICreationTime>().OrderBy(x => x.CreationTime).Cast<TEntity>().AsQueryable();
        }

        return (await query.ApplyToPagedListAsync(QueryOptions)).Select(x => Mapper.Map<TGet>(x));
    }

    [HttpGet("{id}")]
    public virtual Task<QueryResult<TGet>> GetById(TKey id)
    {
        return Repository.GetByIdAsync(id).MapToAsync<TEntity, TGet>().ToQueryResultAsync();
    }

    [HttpPost]
    public virtual Task<QueryResult<TGet>> AddAsync([FromBody] TAdd request)
    {
        return Repository.InsertAsync(request.MapTo<TEntity>()).MapToAsync<TEntity, TGet>().ToQueryResultAsync();
    }

    [HttpPut("{id}")]
    public virtual Task<QueryResult<TGet>> EditAsync(TKey id, [FromBody] TEdit request)
    {
        return Repository.UpdateAsync(id, request).MapToAsync<TEntity, TGet>().ToQueryResultAsync();
    }

    [HttpPatch("{id}")]
    public virtual async Task<QueryResult<TGet>> PatchAsync(TKey id, [FromBody] object jsonData)
    {
        var entity = await Repository.GetByIdAsync(id);
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData.ToString());
        entity = await Repository.UpdateAsync(data.Patch(entity));
        return entity.MapTo<TGet>().ToQueryResult();
    }

    [HttpDelete("{id}")]
    public virtual Task<QueryResult<TGet>> Remove(TKey id)
    {
        return Repository.DeleteAsync(id).MapToAsync<TEntity, TGet>().ToQueryResultAsync();
    }
}

public abstract class CrudController<TEntity> : CrudController<TEntity, Guid, TEntity, TEntity, TEntity> where TEntity : class, IEntity<Guid>;

public abstract class CrudController<TEntity, TKey> : CrudController<TEntity, TKey, TEntity, TEntity, TEntity> where TEntity : class, IEntity<TKey>;

public abstract class CrudController<TEntity, TKey, TGet> : CrudController<TEntity, TKey, TGet, TEntity, TEntity> where TEntity : class, IEntity<TKey>;

public abstract class CrudController<TEntity, TKey, TGet, TAdd> : CrudController<TEntity, TKey, TGet, TAdd, TEntity> where TEntity : class, IEntity<TKey>;