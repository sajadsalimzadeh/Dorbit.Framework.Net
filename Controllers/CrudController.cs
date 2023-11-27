using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Models;
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
    [Auth("[entity]-Select")]
    public virtual async Task<PagedListResult<TGet>> Select(ODataQueryOptions<TEntity> queryOptions)
    {
        return (await Repository.Set().ApplyToPagedListAsync(queryOptions)).Select(x => Mapper.Map<TGet>(x));
    }

    [HttpGet("{id}")]
    [Auth("[entity]-Get")]
    public virtual Task<QueryResult<TGet>> GetById(Guid id)
    {
        return Repository.GetByIdAsync(id).MapAsync<TEntity, TGet>().ToQueryResultAsync();
    }

    [HttpPost]
    [Auth("[entity]-Save")]
    public virtual Task<QueryResult<TGet>> Add([FromBody] TAdd dto)
    {
        return Repository.InsertAsync(dto.MapTo<TEntity>()).MapAsync<TEntity, TGet>().ToQueryResultAsync();
    }

    [HttpPatch("{id}")]
    [Auth("[entity]-Save")]
    public virtual Task<QueryResult<TGet>> Update(Guid id, [FromBody] TEdit dto)
    {
        dto.Id = id;
        return Repository.UpdateAsync(id, dto).MapAsync<TEntity, TGet>().ToQueryResultAsync();
    }

    [HttpDelete("{id}")]
    [Auth("[entity]-Remove")]
    public virtual Task<QueryResult<TGet>> Remove(Guid id)
    {
        return Repository.RemoveAsync(id).MapAsync<TEntity, TGet>().ToQueryResultAsync();
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