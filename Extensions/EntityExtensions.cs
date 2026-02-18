using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Contracts.Abstractions;
using Dorbit.Framework.Contracts.Entities;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Exceptions;

namespace Dorbit.Framework.Extensions;

public static class EntityExtensions
{
    private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> AllReadonlyProperties = new();

    public static TEntity IncludeAudit<TEntity>(this TEntity entity, LogAction action, IUserDto user) where TEntity : IAuditHistory
    {
        if (user is not null)
        {
            entity.Audits ??= [];
            entity.Audits.Add(new Audit()
            {
                UserId = user.GetId(),
                Action = action,
                Time = DateTime.UtcNow,
            });
        }

        return entity;
    }

    public static TEntity IncludeCreationTime<TEntity>(this TEntity entity) where TEntity : ICreationTime
    {
        entity.CreationTime = DateTime.UtcNow;
        if (entity is IModificationTime modificationTime) modificationTime.IncludeModificationTime();
        return entity;
    }

    public static TEntity IncludeCreationAudit<TEntity>(this TEntity entity, IUserDto user) where TEntity : ICreationAudit
    {
        entity.IncludeCreationTime();
        entity.CreatorId = user?.GetId().ToString();
        entity.CreatorName = user?.GetUsername();
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
        entity.ModifierId = user?.GetId().ToString();
        entity.ModifierName = user?.GetUsername();
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
        entity.DeleterId = user?.GetId().ToString();
        entity.DeleterName = user?.GetUsername();
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

    public static TEntity IncludeTenantAudit<TEntity>(this TEntity entity, ITenantDto tenant) where TEntity : ITenantAudit
    {
        entity.TenantId = tenant?.GetId();
        entity.TenantName = tenant?.GetName();

        return entity;
    }

    public static TEntity IncludeServerAudit<TEntity>(this TEntity entity, IServerDto server) where TEntity : IServerAudit
    {
        entity.ServerId = server?.GetId();
        entity.ServerName = server?.GetName();

        return entity;
    }

    public static TEntity IncludeSoftwareAudit<TEntity>(this TEntity entity, ISoftwareDto software) where TEntity : ISoftwareAudit
    {
        entity.SoftwareId = software?.GetId();
        entity.SoftwareName = software?.GetName();
        return entity;
    }

    public static TEntity IncludeChangeLogs<TEntity>(this TEntity entity, TEntity oldEntity = default) where TEntity : IChangeLog
    {
        entity.ChangeLogs ??= new List<ChangeLogDto>();
        var properties = entity.GetType().GetProperties();
        foreach (var propertyInfo in properties.Where(x => x.GetCustomAttribute<ChangeLogAttribute>() is not null))
        {
            entity.ChangeLogs.Add(new ChangeLogDto()
            {
                Timestamp = DateTime.UtcNow,
                Property = propertyInfo.Name,
                OldValue = (oldEntity is null ? null : propertyInfo.GetValue(oldEntity)?.ToString()),
                NewValue = propertyInfo.GetValue(entity)?.ToString(),
            });
        }

        return entity;
    }

    public static TEntity IncludeChangeLogs<TEntity, TKey>(this TEntity entity, TEntity oldEntity = default) where TEntity : IChangeLog
    {
        entity.IncludeChangeLogs(oldEntity);
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
        if (entity is IValidate validator) validator.Validate(e);
        if (entity is ICreationValidate creationValidator) creationValidator.ValidateOnCreate(e);
        e.ThrowIfHasError();
        return entity;
    }

    public static TEntity ValidateModification<TEntity, TKey>(this TEntity entity) where TEntity : IEntity<TKey>
    {
        var e = new ModelValidationException();
        if (entity is IValidate validator) validator.Validate(e);
        if (entity is IModificationValidate modificationValidator) modificationValidator.ValidateOnModify(e);
        e.ThrowIfHasError();
        return entity;
    }

    public static TEntity ValidateDeletion<TEntity, TKey>(this TEntity entity) where TEntity : IEntity<TKey>
    {
        var e = new ModelValidationException();
        if (entity is IValidate validator) validator.Validate(e);
        if (entity is IDeletionValidate deletionValidator) deletionValidator.ValidateOnDelete(e);
        e.ThrowIfHasError();
        return entity;
    }

    public static List<PropertyInfo> GetReadonlyProperties<TEntity, TKey>(this TEntity entity) where TEntity : IEntity<TKey>
    {
        return AllReadonlyProperties.GetOrAdd(typeof(TEntity), _ =>
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

    public static void SetStatusLog<T, TStatus>(this T entity, TStatus status, Guid? userId = null, string description = null) where T : IStatusLog<TStatus> where TStatus : Enum
    {
        entity.Status = status;
        entity.StatusLogs ??= [];
        entity.StatusLogs.Add(new StatusLog<TStatus>()
        {
            Status = status,
            Time = DateTime.UtcNow,
            UserId = userId,
            Description = description,
        });
    }

    public static void OwnerRequest<T>(this T entity, Guid userId) where T : IOwnerLog
    {
        entity.OwnerRequestId = userId;
        entity.OwnerRequestTime = DateTime.UtcNow;
    }

    public static void OwnerRequestCancel<T>(this T entity) where T : IOwnerLog
    {
        entity.OwnerRequestTime = null;
        entity.OwnerRequestId = null;
    }

    public static void OwnerRequestDecide<T>(this T entity, Guid userId, OwnerDecideRequest request) where T : IOwnerLog
    {
        entity.OwnerLogs ??= [];
        if (entity.OwnerRequestTime.HasValue)
        {
            entity.OwnerLogs.Add(new OwnerLog()
            {
                UserId = userId,
                RequestTime = entity.OwnerRequestTime.Value,
                DecideTime = DateTime.UtcNow,
                IsAccept = request.IsAccept,
                Description = request.Description
            });
        }

        if (request.IsAccept)
        {
            entity.OwnerId = userId;
        }

        entity.OwnerRequestCancel();
    }
}