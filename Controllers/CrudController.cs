using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

public abstract class CrudController<TEntity, TKey, TGet, TAdd> : CrudController
    where TEntity : class, IEntity<TKey>
{
    private static readonly HashSet<string> ProtectedProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "IsDeleted",
        "CreatorId", "CreatorName", "CreationTime",
        "ModifierId", "ModifierName", "ModificationTime",
        "DeleterId", "DeleterName", "DeletionTime",
        "TenantId", "TenantName",
        "ServerId", "ServerName",
        "SoftwareId", "SoftwareName"
    };

    protected IBaseRepository<TEntity, TKey> Repository => ServiceProvider.GetRequiredService<IBaseRepository<TEntity, TKey>>();
    
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
    public virtual Task<QueryResult<TGet>> GetByIdAsync(TKey id)
    {
        return Repository.GetByIdAsync(id).MapToAsync<TEntity, TGet>().ToQueryResultAsync();
    }

    [HttpPost, Auth("{type0}-Save", "{type0}-Add")]
    public virtual Task<QueryResult<TGet>> AddAsync([FromBody] TAdd request)
    {
        ClearProtectedCreateValues(request);
        MemoryCache.Remove(typeof(TEntity));
        return Repository.InsertAsync(request.MapTo<TEntity>()).MapToAsync<TEntity, TGet>().ToQueryResultAsync();
    }

    [HttpPatch("{id}"), HttpPut("{id}"), Auth("{type0}-Save", "{type0}-Edit")]
    public virtual async Task<QueryResult<TGet>> PatchAsync(TKey id, [FromBody] JsonElement obj)
    {
        MemoryCache.Remove(id.ToString() ?? string.Empty);
        MemoryCache.Remove(typeof(TEntity));
        var entity = await Repository.UpdateWithJsonAsync<TEntity>(id, WithoutProtectedProperties(obj));
        return entity.MapTo<TGet>().ToQueryResult();
    }

    private static void ClearProtectedCreateValues(object request)
    {
        if (request is null) return;
        foreach (var name in new[] { "IsDeleted", "TenantId", "TenantName", "DeleterId", "DeleterName", "DeletionTime" })
        {
            var property = request.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property is null || !property.CanWrite) continue;
            var defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;
            property.SetValue(request, defaultValue);
        }
    }

    private static JsonElement WithoutProtectedProperties(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object) return element;

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var property in element.EnumerateObject())
            {
                if (ProtectedProperties.Contains(property.Name)) continue;
                property.WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        using var document = JsonDocument.Parse(stream.ToArray());
        return document.RootElement.Clone();
    }

    [HttpDelete("{id}"), Auth("{type0}-Delete")]
    public virtual async Task<CommandResult> DeleteAsync(TKey id)
    {
        MemoryCache.Remove(id.ToString() ?? string.Empty);
        MemoryCache.Remove(typeof(TEntity));
        await Repository.DeleteAsync(id);
        return Succeed();
    }
}

public abstract class CrudController<TEntity> : CrudController<TEntity, Guid, TEntity, TEntity> where TEntity : class, IEntity<Guid>;

public abstract class CrudController<TEntity, TKey> : CrudController<TEntity, TKey, TEntity, TEntity> where TEntity : class, IEntity<TKey>;

public abstract class CrudController<TEntity, TKey, TGet> : CrudController<TEntity, TKey, TGet, TEntity> where TEntity : class, IEntity<TKey>;