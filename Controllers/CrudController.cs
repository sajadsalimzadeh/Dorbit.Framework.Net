using System;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Repositories.Abstractions;
using Dorbit.Framework.Utils.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Controllers;

public abstract class CrudController : BaseController;

public abstract class CrudController<TEntity, TKey, TGet, TAdd, TEdit> : CrudController where TEntity : class, IEntity<TKey> where TEdit : IEntity<TKey>
{
    protected IBaseRepository<TEntity, TKey> Repository => ServiceProvider.GetService<IBaseRepository<TEntity, TKey>>();

    [HttpGet]
    public virtual async Task<PagedListResult<TGet>> SelectAsync()
    {
        return (await Repository.Set().ApplyToPagedListAsync(QueryOptions)).Select(x => Mapper.Map<TGet>(x));
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

    [HttpPatch("{id}")]
    public virtual Task<QueryResult<TGet>> EditAsync(TKey id, [FromBody] TEdit request)
    {
        request.Id = id;
        return Repository.UpdateAsync(id, request).MapToAsync<TEntity, TGet>().ToQueryResultAsync();
    }

    [HttpDelete("{id}")]
    public virtual Task<QueryResult<TGet>> Remove(TKey id)
    {
        return Repository.DeleteAsync(id).MapToAsync<TEntity, TGet>().ToQueryResultAsync();
    }
}

public abstract class CrudController<TEntity> : CrudController<TEntity, Guid, TEntity, TEntity, TEntity> 
    where TEntity : class, IEntity<Guid>;

public abstract class CrudController<TEntity, TKey> : CrudController<TEntity, TKey, TEntity, TEntity, TEntity> 
    where TEntity : class, IEntity<TKey>;

public abstract class CrudController<TEntity, TKey, TGet> : CrudController<TEntity, TKey, TGet, TEntity, TEntity>
    where TEntity : class, IEntity<TKey>;

public abstract class CrudController<TEntity, TKey, TGet, TAdd> : CrudController<TEntity, TKey, TGet, TAdd, TEntity> 
    where TEntity : class, IEntity<TKey>;