using System;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Repositories.Abstractions;
using Dorbit.Framework.Utils.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Controllers;

public abstract class CrudController : BaseController
{
}

public abstract class CrudController<TEntity, TGet, TAdd, TEdit> : CrudController where TEntity : class, IEntity where TEdit : IEntity
{
    protected IBaseRepository<TEntity> Repository => ServiceProvider.GetService<IBaseRepository<TEntity>>();

    [HttpGet]
    public virtual async Task<PagedListResult<TGet>> SelectAsync()
    {
        return (await Repository.Set().ApplyToPagedListAsync(QueryOptions)).Select(x => Mapper.Map<TGet>(x));
    }

    [HttpGet("{id}")]
    public virtual Task<QueryResult<TGet>> GetById(Guid id)
    {
        return Repository.GetByIdAsync(id).MapToAsync<TEntity, TGet>().ToQueryResultAsync();
    }

    [HttpPost]
    public virtual Task<QueryResult<TGet>> AddAsync([FromBody] TAdd dto)
    {
        return Repository.InsertAsync(dto.MapTo<TEntity>()).MapToAsync<TEntity, TGet>().ToQueryResultAsync();
    }

    [HttpPatch("{id}")]
    public virtual Task<QueryResult<TGet>> EditAsync(Guid id, [FromBody] TEdit dto)
    {
        dto.Id = id;
        return Repository.UpdateAsync(id, dto).MapToAsync<TEntity, TGet>().ToQueryResultAsync();
    }

    [HttpDelete("{id}")]
    public virtual Task<QueryResult<TGet>> Remove(Guid id)
    {
        return Repository.RemoveAsync(id).MapToAsync<TEntity, TGet>().ToQueryResultAsync();
    }
}

public abstract class CrudController<TEntity> : CrudController<TEntity, TEntity, TEntity, TEntity> where TEntity : class, IEntity
{
}

public abstract class CrudController<TEntity, TGet> : CrudController<TEntity, TGet, TEntity, TEntity> where TEntity : class, IEntity
{
}

public abstract class CrudController<TEntity, TGet, TAdd> : CrudController<TEntity, TGet, TAdd, TEntity> where TEntity : class, IEntity
{
}