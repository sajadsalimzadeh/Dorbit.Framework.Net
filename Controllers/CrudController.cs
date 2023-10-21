using Dorbit.Entities.Abstractions;
using Dorbit.Filters;
using Dorbit.Models;
using Dorbit.Repositories.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Controllers
{
    public abstract class CrudController : BaseController
    {

    }

    public abstract class CrudController<TEntity, TGetDto, TAddDto, TUpdateDto> :
        CrudController where TEntity : class, IEntity where TUpdateDto : IEntity
    {
        protected IBaseRepository<TEntity> Repository => ServiceProvider.GetService<IBaseRepository<TEntity>>();

        [HttpGet]
        [Auth("[entity]-Select")]
        public virtual PagedListResult<TGetDto> Select()
        {
            return Repository.Select(QueryOptions).Select(x => Mapper.Map<TGetDto>(x));
        }

        [HttpGet("{id}")]
        [Auth("[entity]-Select")]
        public virtual QueryResult<TGetDto> GetById(long id)
        {
            return Mapper.Map<TGetDto>(Repository.GetById(id)).ToQueryResult();
        }

        [HttpPost]
        [Auth("[entity]-Save")]
        public virtual QueryResult<TGetDto> Add([FromBody] TAddDto dto)
        {
            return Mapper.Map<TGetDto>(Repository.Insert(Mapper.Map<TEntity>(dto))).ToQueryResult();
        }

        [HttpPatch("{id}")]
        [Auth("[entity]-Save")]
        public virtual QueryResult<TGetDto> Update(long id, [FromBody] TUpdateDto dto)
        {
            dto.Id = id;
            return Mapper.Map<TGetDto>(Repository.Update(id, dto)).ToQueryResult();
        }

        [HttpDelete("{id}")]
        [Auth("[entity]-Remove")]
        public virtual QueryResult<TGetDto> Remove(long id)
        {
            return Mapper.Map<TGetDto>(Repository.Remove(id)).ToQueryResult();
        }
    }

    public abstract class CrudController<TEntity> : CrudController<TEntity, TEntity, TEntity, TEntity> where TEntity : class, IEntity
    {

    }
}
