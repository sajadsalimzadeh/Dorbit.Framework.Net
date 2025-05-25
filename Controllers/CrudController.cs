using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Repositories.Abstractions;
using Dorbit.Framework.Utils.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Controllers;

public abstract class CrudController : BaseController;

public abstract class CrudController<TEntity, TKey, TGet, TAdd, TEdit> : CrudController
    where TEntity : class, IEntity<TKey>
{
    protected IBaseRepository<TEntity, TKey> Repository => ServiceProvider.GetService<IBaseRepository<TEntity, TKey>>();
    
    protected virtual IQueryable<TEntity> Set() => Repository.Set();

    [HttpGet, Auth("{type0}-View")]
    public virtual async Task<QueryResult<IEnumerable<TGet>>> GetAllAsync()
    {
        var query = Set();
        if (typeof(TEntity).IsAssignableTo(typeof(ICreationTime)))
        {
            query = query.Cast<ICreationTime>().OrderBy(x => x.CreationTime).Cast<TEntity>().AsQueryable();
        }

        var result = await query.ToListAsync();
        return result.Select(x => Mapper.Map<TGet>(x)).ToQueryResult();
    }
    
    [HttpGet("odata"), Auth("{type0}-View")]
    public virtual async Task<PagedListResult<TGet>> SelectAsync()
    {
        var query = Set();
        if (typeof(TEntity).IsAssignableTo(typeof(ICreationTime)))
        {
            query = query.Cast<ICreationTime>().OrderBy(x => x.CreationTime).Cast<TEntity>().AsQueryable();
        }

        return (await query.ApplyToPagedListAsync(QueryOptions)).Select(x => Mapper.Map<TGet>(x));
    }

    [HttpGet("{id}"), Auth("{type0}-View")]
    public virtual Task<QueryResult<TGet>> GetById(TKey id)
    {
        return Repository.GetByIdAsync(id).MapToAsync<TEntity, TGet>().ToQueryResultAsync();
    }

    [HttpPost, Auth("{type0}-Save")]
    public virtual Task<QueryResult<TGet>> AddAsync([FromBody] TAdd request)
    {
        return Repository.InsertAsync(request.MapTo<TEntity>()).MapToAsync<TEntity, TGet>().ToQueryResultAsync();
    }

    [HttpPut("{id}"), HttpPatch("{id}"), Auth("{type0}-Save")]
    public virtual async Task<QueryResult<TGet>> PatchAsync(TKey id, [FromBody] JsonElement obj)
    {
        var entity = await Repository.PatchAsync(id, obj);
        return entity.MapTo<TGet>().ToQueryResult();
    }

    [HttpDelete("{id}"), Auth("{type0}-Delete")]
    public virtual async Task<CommandResult> Remove(TKey id)
    {
        await Repository.DeleteAsync(id);
        return Succeed();
    }
}

public abstract class CrudController<TEntity> : CrudController<TEntity, Guid, TEntity, TEntity, TEntity> where TEntity : class, IEntity<Guid>;

public abstract class CrudController<TEntity, TKey> : CrudController<TEntity, TKey, TEntity, TEntity, TEntity> where TEntity : class, IEntity<TKey>;

public abstract class CrudController<TEntity, TKey, TGet> : CrudController<TEntity, TKey, TGet, TEntity, TEntity> where TEntity : class, IEntity<TKey>;

public abstract class CrudController<TEntity, TKey, TGet, TAdd> : CrudController<TEntity, TKey, TGet, TAdd, TEntity> where TEntity : class, IEntity<TKey>;