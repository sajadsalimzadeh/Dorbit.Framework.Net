using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Abstractions;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Exceptions;

namespace Dorbit.Framework.Extensions;

public static class EntityExtensions
{
    
    private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> _allReadonlyProperties = new();
    
    public static TEntity IncludeCreationTime<TEntity>(this TEntity entity) where TEntity : ICreationTime
    {
        entity.CreationTime = DateTime.UtcNow;
        if (entity is IModificationTime modificationTime) modificationTime.IncludeModificationTime();
        return entity;
    }

    public static TEntity IncludeCreationAudit<TEntity>(this TEntity entity, IUserDto user) where TEntity : ICreationAudit
    {
        entity.IncludeCreationTime();
        entity.CreatorId = user?.Id?.ToString();
        entity.CreatorName = user?.Username;
        return entity;
    }

    public static TEntity IncludeModificationTime<TEntity>(this TEntity entity) where TEntity : IModificationTime
    {
        entity.ModificationTime = DateTime.UtcNow;
        return entity;
    }

    public static TEntity IncludeModificationAudit<TEntity>(this TEntity entity, IUserDto user) where TEntity : IModificationAudit
    {
        entity.IncludeModificationTime();
        entity.ModifierId = user?.Id?.ToString();
        entity.ModifierName = user?.Username;
        return entity;
    }

    public static TEntity IncludeDeletionTime<TEntity>(this TEntity entity) where TEntity : IDeletionTime
    {
        entity.DeletionTime = DateTime.UtcNow;
        return entity;
    }

    public static TEntity IncludeDeletionAudit<TEntity>(this TEntity entity, IUserDto user) where TEntity : IDeletionAudit
    {
        entity.IncludeDeletionTime();
        entity.DeleterId = user?.Id?.ToString();
        entity.DeleterName = user?.Username;
        return entity;
    }

    public static TEntity IncludeCreationAudit<TEntity, TKey>(this TEntity entity, IUserDto user) where TEntity : IEntity<TKey>
    {
        if (entity is ICreationAudit creationAudit) creationAudit.IncludeCreationAudit(user);
        else if (entity is ICreationTime creationTime) creationTime.IncludeCreationTime();
        return entity;
    }

    public static TEntity IncludeModificationAudit<TEntity, TKey>(this TEntity entity, IUserDto user) where TEntity : IEntity<TKey>
    {
        if (entity is IModificationAudit modificationAudit) modificationAudit.IncludeModificationAudit(user);
        else if (entity is IModificationTime modificationTime) modificationTime.IncludeModificationTime();
        return entity;
    }

    public static TEntity IncludeDeletionAudit<TEntity, TKey>(this TEntity entity, IUserDto user) where TEntity : IEntity<TKey>
    {
        if (entity is IDeletionAudit deletionAudit) deletionAudit.IncludeDeletionAudit(user);
        else if (entity is IDeletionTime deletionTime) deletionTime.IncludeDeletionTime();
        return entity;
    }

    public static TEntity IncludeTenantAudit<TEntity, TKey>(this TEntity entity, ITenantDto tenant) where TEntity : IEntity<TKey>
    {
        if (entity is ITenantAudit tenantAudit)
        {
            tenantAudit.TenantId = tenant?.Id;
            tenantAudit.TenantName = tenant?.Name;
        }

        return entity;
    }

    public static TEntity IncludeServerAudit<TEntity, TKey>(this TEntity entity, IServerDto server) where TEntity : IEntity<TKey>
    {
        if (entity is IServerAudit serverAudit)
        {
            serverAudit.ServerId = server?.Id;
            serverAudit.ServerName = server?.Name;
        }

        return entity;
    }

    public static TEntity IncludeSoftwareAudit<TEntity, TKey>(this TEntity entity, ISoftwareDto software) where TEntity : IEntity<TKey>
    {
        if (entity is ISoftwareAudit softwareAudit)
        {
            softwareAudit.SoftwareId = software?.Id;
            softwareAudit.SoftwareName = software?.Name;
        }

        return entity;
    }

    public static TEntity IncludeChangeLogs<TEntity>(this TEntity entity, TEntity oldEntity = default) where TEntity : IChangeLog
    {
        entity.ChangeLogs ??= new List<ChangeLog>();
        var properties = entity.GetType().GetProperties();
        foreach (var propertyInfo in properties.Where(x => x.GetCustomAttribute<ChangeLogAttribute>() is not null))
        {
            entity.ChangeLogs.Add(new ChangeLog()
            {
                Timestamp = DateTime.UtcNow,
                Property = propertyInfo.Name,
                OldValue = (oldEntity is null ? null : propertyInfo.GetValue(oldEntity)?.ToString()),
                NewValue = propertyInfo.GetValue(entity)?.ToString(),
            });
        }

        return entity;
    }

    public static TEntity IncludeChangeLogs<TEntity, TKey>(this TEntity entity, TEntity oldEntity = default) where TEntity : IEntity<TKey>
    {
        if (entity is IChangeLog changeLog) changeLog.IncludeChangeLogs(oldEntity as IChangeLog);
        return entity;
    }
    
    public static TEntity GenerateHistoricalId<TEntity, TKey>(this TEntity entity) where TEntity : IEntity<TKey>
    {
        if (entity is IHistorical historical)
        {
            historical.IsHistorical = false;
            historical.HistoryId = Guid.NewGuid();
        }

        return entity;
    }
    
    public static TEntity IncludeVersionAudit<TEntity, TKey>(this TEntity entity) where TEntity : IEntity<TKey>
    {
        if (entity is IVersionAudit versionAudit)
        {
            versionAudit.Version++;
        }

        return entity;
    }

    public static TEntity GenerateKey<TEntity, TKey>(this TEntity entity) where TEntity : IEntity<TKey>
    {
        if (entity is IEntity<Guid> guidEntity)
        {
            if (guidEntity.Id == Guid.Empty) guidEntity.Id = Guid.NewGuid();
        }

        return entity;
    }

    public static TEntity ValidateCreation<TEntity, TKey>(this TEntity entity) where TEntity : IEntity<TKey>
    {
        var e = new ModelValidationException();
        if (entity is IValidator validator) validator.Validate(e);
        if (entity is ICreationValidator creationValidator) creationValidator.ValidateOnCreate(e);
        e.ThrowIfHasError();
        return entity;
    }

    public static TEntity ValidateModification<TEntity, TKey>(this TEntity entity) where TEntity : IEntity<TKey>
    {
        var e = new ModelValidationException();
        if (entity is IValidator validator) validator.Validate(e);
        if (entity is IModificationValidator modificationValidator) modificationValidator.ValidateOnModify(e);
        e.ThrowIfHasError();
        return entity;
    }

    public static TEntity ValidateDeletion<TEntity, TKey>(this TEntity entity) where TEntity : IEntity<TKey>
    {
        var e = new ModelValidationException();
        if (entity is IValidator validator) validator.Validate(e);
        if (entity is IDeletionValidator deletionValidator) deletionValidator.ValidateOnDelete(e);
        e.ThrowIfHasError();
        return entity;
    }

    public static List<PropertyInfo> GetReadonlyProperties<TEntity, TKey>(this TEntity entity) where TEntity : IEntity<TKey>
    {
        return _allReadonlyProperties.GetOrAdd(typeof(TEntity), _ =>
        {
            var items = new List<PropertyInfo>();
            var properties = entity.GetType().GetProperties();
            foreach (var property in properties)
            {
                var readonlyAttr = property.GetCustomAttribute<ReadOnlyAttribute>();
                if (readonlyAttr is null || !readonlyAttr.IsReadOnly) continue;
                items.Add(property);
            }

            return items;
        });       
    }
}