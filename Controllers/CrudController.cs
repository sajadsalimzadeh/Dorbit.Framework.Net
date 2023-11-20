using Dorbit.Entities.Abstractions;
using Dorbit.Extensions;
using Dorbit.Filters;
using Dorbit.Models;
using Dorbit.Repositories.Abstractions;
using Dorbit.Utils.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Controllers;

public abstract class CrudController : BaseController
{

}

public abstract class CrudController<TEntity, TGetDto, TAddDto, TUpdateDto> :
    CrudController where TEntity : class, IEntity where TUpdateDto : IEntity
{
    protected IBaseRepository<TEntity> Repository => ServiceProvider.GetService<IBaseRepository<TEntity>>();

    [HttpGet]
    [Auth("[entity]-Select")]
    public virtual async Task<PagedListResult<TGetDto>> Select()
    {
        return (await Repository.Set().ApplyToPagedListAsync(QueryOptions)).Select(x => Mapper.Map<TGetDto>(x));
    }

    [HttpGet("{id}")]
    [Auth("[entity]-Select")]
    public virtual Task<QueryResult<TGetDto>> GetById(Guid id)
    {
        return Repository.GetByIdAsync(id).MapTo<TGetDto>().ToQueryResultAsync();
    }

    [HttpPost]
    [Auth("[entity]-Save")]
    public virtual Task<QueryResult<TGetDto>> Add([FromBody] TAddDto dto)
    {
        return Repository.InsertAsync(dto.MapTo<TEntity>()).MapTo<TGetDto>().ToQueryResultAsync();
    }

    [HttpPatch("{id}")]
    [Auth("[entity]-Save")]
    public virtual Task<QueryResult<TGetDto>> Update(Guid id, [FromBody] TUpdateDto dto)
    {
        dto.Id = id;
        return Repository.UpdateAsync(id, dto).MapTo<TGetDto>().ToQueryResultAsync();
    }

    [HttpDelete("{id}")]
    [Auth("[entity]-Remove")]
    public virtual Task<QueryResult<TGetDto>> Remove(Guid id)
    {
        return Repository.RemoveAsync(id).MapTo<TGetDto>().ToQueryResultAsync();
    }
}

public abstract class CrudController<TEntity> : CrudController<TEntity, TEntity, TEntity, TEntity> where TEntity : class, IEntity
{

}