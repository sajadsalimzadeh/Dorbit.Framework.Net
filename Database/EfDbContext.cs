using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Contracts.Loggers;
using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Hosts;
using Dorbit.Framework.Services.Abstractions;
using EFCore.BulkExtensions;
using Innofactor.EfCoreJsonValueConverter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Dorbit.Framework.Database;

public abstract class EfDbContext : DbContext, IDbContext
{
    private static readonly List<string> Sequences = [];
    private static readonly ConcurrentDictionary<string, int> SequenceCounter = [];
    private readonly EfTransactionContext _efTransactionContext;

    private IUserResolver _userResolver;
    private IUserResolver UserResolver => _userResolver ??= ServiceProvider.GetService<IUserResolver>();

    private ITenantResolver _tenantResolver;
    private ITenantResolver TenantResolver => _tenantResolver ??= ServiceProvider.GetService<ITenantResolver>();

    private IServerResolver _serverResolver;
    private IServerResolver ServerResolver => _serverResolver ??= ServiceProvider.GetService<IServerResolver>();

    private ISoftwareResolver _softwareResolver;
    private ISoftwareResolver SoftwareResolver => _softwareResolver ??= ServiceProvider.GetService<ISoftwareResolver>();

    private ILogger _logger;
    private ILogger Logger => _logger ??= ServiceProvider.GetService<ILogger>();

    private LoggerHostInterval _loggerHostInterval;
    private LoggerHostInterval LoggerHostInterval => _loggerHostInterval ??= ServiceProvider.GetService<LoggerHostInterval>();

    public IServiceProvider ServiceProvider { get; }
    public CancellationToken CancellationToken { get; set; }
    public bool AutoExcludeDeleted { get; set; } = true;

    protected EfDbContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options)
    {
        ServiceProvider = serviceProvider;
        _efTransactionContext = new EfTransactionContext(this);
        CancellationToken = serviceProvider.GetService<ICancellationTokenService>()?.CancellationToken ?? default;
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddJsonFields();

        base.OnModelCreating(modelBuilder);

        RegisterAuditProperties(modelBuilder);

        foreach (var type in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var keys in type.GetForeignKeys().Where(x => x is { IsOwnership: false, DeleteBehavior: DeleteBehavior.Cascade }))
            {
                keys.DeleteBehavior = DeleteBehavior.NoAction;
            }

            foreach (var property in type.ClrType.GetProperties())
            {
                var sequenceAttribute = property.GetCustomAttribute<SequenceAttribute>();
                if (sequenceAttribute is null) continue;
                if (!Sequences.Contains(sequenceAttribute.Name))
                {
                    modelBuilder.Entity(type.ClrType).Property(property.Name).ValueGeneratedOnAdd();
                    // modelBuilder.HasSequence<int>(sequenceAttribute.Name, schema: sequenceAttribute.Schema)
                    //     .StartsAt(sequenceAttribute.StartAt)
                    //     .IncrementsBy(sequenceAttribute.IncrementsBy);
                    // Sequences.Add(sequenceAttribute.Name);
                }

                // modelBuilder.Entity(type.ClrType).Property(property.Name).HasDefaultValueSql($"NEXT VALUE FOR {sequenceAttribute.Name}");
            }
        }
    }

    protected DatabaseProviderType GetProvider()
    {
        var providerName = Database.ProviderName?.ToLower();
        if (providerName == null) return DatabaseProviderType.Unknown;
        if (providerName.Contains("inmemory")) return DatabaseProviderType.InMemory;
        if (providerName.Contains("mysql")) return DatabaseProviderType.MySql;
        if (providerName.Contains("sqlserver")) return DatabaseProviderType.SqlServer;
        if (providerName.Contains("postgressql")) return DatabaseProviderType.Postgres;
        return DatabaseProviderType.Unknown;
    }

    private void RegisterAuditProperties(ModelBuilder modelBuilder)
    {
        foreach (var type in modelBuilder.Model.GetEntityTypes().Select(x => x.ClrType))
        {
            var entity = modelBuilder.Entity(type);
            if (typeof(ICreationAudit).IsAssignableFrom(type))
                entity.Property(nameof(ICreationAudit.CreatorName)).HasMaxLength(256);

            if (typeof(IModificationAudit).IsAssignableFrom(type))
                entity.Property(nameof(IModificationAudit.ModifierName)).HasMaxLength(256);

            if (typeof(IDeletionAudit).IsAssignableFrom(type))
                entity.Property(nameof(IDeletionAudit.DeleterName)).HasMaxLength(256);

            if (typeof(ITenantAudit).IsAssignableFrom(type))
                entity.Property(nameof(ITenantAudit.TenantName)).HasMaxLength(256);

            if (typeof(IServerAudit).IsAssignableFrom(type))
                entity.Property(nameof(IServerAudit.ServerName)).HasMaxLength(256);

            if (typeof(ISoftwareAudit).IsAssignableFrom(type))
                entity.Property(nameof(ISoftwareAudit.SoftwareName)).HasMaxLength(256);
        }
    }

    public ITransaction BeginTransaction()
    {
        if (GetProvider() == DatabaseProviderType.InMemory) return new InMemoryTransaction(this);
        return _efTransactionContext.BeginTransaction();
    }

    public IQueryable<TEntity> DbSet<TEntity, TKey>(bool? excludeDeleted = null) where TEntity : class, IEntity<TKey>
    {
        excludeDeleted ??= AutoExcludeDeleted;
        var set = Set<TEntity>().AsNoTracking();
        if (!excludeDeleted.Value) return set;

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            set = set.Cast<ISoftDelete>().Where(x => !x.IsDeleted).Cast<TEntity>().AsQueryable();
        }

        return set;
    }

    public IQueryable<TEntity> DbSet<TEntity>(bool? excludeDeleted = null) where TEntity : class, IEntity<Guid>
    {
        return DbSet<TEntity, Guid>(excludeDeleted);
    }

    public async Task<TEntity> InsertEntityAsync<TEntity, TKey>(TEntity model) where TEntity : class, IEntity<TKey>
    {
        if (GetProvider() == DatabaseProviderType.InMemory)
        {
            var type = typeof(TEntity);
            foreach (var property in type.GetProperties())
            {
                var sequenceAttribute = property.GetCustomAttribute<SequenceAttribute>();
                if (sequenceAttribute is null) continue;

                var counter = SequenceCounter.GetOrAdd(sequenceAttribute.Name, _ => sequenceAttribute.StartAt);
                counter += sequenceAttribute.IncrementsBy;
                property.SetValue(model, counter);
            }
        }

        var e = new ModelValidationException();
        if (model is IValidator validator) validator.Validate(e, ServiceProvider);
        if (model is ICreationValidator creationValidator) creationValidator.ValidateOnCreate(e, ServiceProvider);
        e.ThrowIfHasError();
        if (model is ICreationTime creationTime) creationTime.CreationTime = DateTime.UtcNow;
        if (model is IModificationTime modificationTime) modificationTime.ModificationTime = DateTime.UtcNow;
        if (model is ICreationAudit creationAudit)
        {
            var user = UserResolver?.User;
            creationAudit.CreatorId = user?.Id?.ToString();
            creationAudit.CreatorName = user?.Username;
        }

        if (model is ITenantAudit tenantAudit)
        {
            var tenant = TenantResolver?.GetTenant();
            tenantAudit.TenantId = tenant?.Id;
            tenantAudit.TenantName = tenant?.Name;
        }

        if (model is IServerAudit serverAudit)
        {
            var server = ServerResolver?.GetServer();
            serverAudit.ServerId = server?.Id;
            serverAudit.ServerName = server?.Name;
        }

        if (model is ISoftwareAudit softwareAudit)
        {
            var software = SoftwareResolver?.GetSoftware();
            softwareAudit.SoftwareId = software?.Id;
            softwareAudit.SoftwareName = software?.Name;
        }

        if (model is IHistorical historical)
        {
            historical.IsHistorical = false;
            historical.HistoryId = Guid.NewGuid();
        }

        if (model is IEntity<Guid> guidEntity)
        {
            if (guidEntity.Id == Guid.Empty) guidEntity.Id = Guid.NewGuid();
        }
        
        await AddAsync(model, CancellationToken);
        await SaveIfNotInTransactionAsync();
        if (model is ICreationLogging logging) Log(logging, LogAction.Insert);
        return model;
    }

    public Task<TEntity> InsertEntityAsync<TEntity>(TEntity model) where TEntity : class, IEntity<Guid>
    {
        return InsertEntityAsync<TEntity, Guid>(model);
    }

    public async Task<TEntity> UpdateEntityAsync<TEntity, TKey>(TEntity model) where TEntity : class, IEntity<TKey>
    {
        if (model is IReadonly) throw new OperationException(Errors.EntityIsReadonly);

        var e = new ModelValidationException();
        if (model is IValidator validator) validator.Validate(e, ServiceProvider);
        if (model is IModificationValidator modificationValidator) modificationValidator.ValidateOnModify(e, ServiceProvider);
        e.ThrowIfHasError();

        if (model is IVersionAudit versionAudit) versionAudit.Version++;
        if (model is IModificationTime modificationTime) modificationTime.ModificationTime = DateTime.UtcNow;
        if (model is IModificationAudit modificationAudit)
        {
            var user = UserResolver?.User;
            modificationAudit.ModifierId = user?.Id?.ToString();
            modificationAudit.ModifierName = user?.Username;
        }

        if (model is not IHistorical)
        {
            Entry(model).State = EntityState.Modified;
            var properties = model.GetType().GetProperties();
            foreach (var property in properties)
            {
                var readonlyAttr = property.GetCustomAttribute<ReadOnlyAttribute>();
                if (readonlyAttr is null || !readonlyAttr.IsReadOnly) continue;
                Entry(model).Property(property.Name).IsModified = false;
            }

            if (model is ICreationTime creationTime) Entry(creationTime).Property(x => x.CreationTime).IsModified = false;
            if (model is ICreationAudit creationAudit)
            {
                Entry(creationAudit).Property(x => x.CreatorId).IsModified = false;
                Entry(creationAudit).Property(x => x.CreatorName).IsModified = false;
            }
        }
        TEntity oldModel = default;
        if (model is IHistorical)
        {
            oldModel = DbSet<TEntity, TKey>().GetById(model.Id);
            using var transaction = BeginTransaction();
            if (oldModel is IHistorical oldHistoricalModel)
            {
                oldHistoricalModel.IsHistorical = true;
                Entry(oldHistoricalModel).State = EntityState.Modified;
            }

            if (oldModel is ICreationTime creationTime) Entry(creationTime).Property(x => x.CreationTime).IsModified = false;
            if (oldModel is ICreationAudit creationAudit)
            {
                Entry(creationAudit).Property(x => x.CreatorId).IsModified = false;
                Entry(creationAudit).Property(x => x.CreatorName).IsModified = false;
            }

            await InsertEntityAsync<TEntity, TKey>(model);
            await transaction.CommitAsync();
        }

        await SaveIfNotInTransactionAsync();
        if (model is ILogging) Log(model, LogAction.Update, oldModel);
        return model;
    }

    public Task<TEntity> UpdateEntityAsync<TEntity>(TEntity model) where TEntity : class, IEntity<Guid>
    {
        return UpdateEntityAsync<TEntity, Guid>(model);
    }
    
    public async Task<TEntity> DeleteEntityAsync<TEntity, TKey>(TEntity entity) where TEntity : class, IEntity<TKey>
    {
        if (entity is IUnDeletable) throw new OperationException(Errors.EntityIsUnDeletable);

        var e = new ModelValidationException();
        if (entity is IValidator validator) validator.Validate(e, ServiceProvider);
        if (entity is IDeletationValidator deletionValidator) deletionValidator.ValidateOnDelete(e, ServiceProvider);
        e.ThrowIfHasError();

        if (entity is ISoftDelete softDelete)
        {
            if (!softDelete.IsDeleted)
            {
                softDelete.IsDeleted = true;

                if (softDelete is IDeletionTime deletionTime) deletionTime.DeletionTime = DateTime.UtcNow;
                if (softDelete is IDeletionAudit deletionAudit)
                {
                    var user = UserResolver?.User;
                    deletionAudit.DeleterId = user?.Id?.ToString();
                    deletionAudit.DeleterName = user?.Username;
                }

                Entry(softDelete).State = EntityState.Modified;
            }
            else
            {
                Entry(softDelete).State = EntityState.Detached;
            }
        }
        else
        {
            Entry(entity).State = EntityState.Deleted;
        }
        
        await SaveIfNotInTransactionAsync();
        if (entity is ILogging logging) Log(logging, LogAction.Delete);
        return entity;
    }

    public Task<TEntity> DeleteEntityAsync<TEntity>(TEntity model) where TEntity : class, IEntity<Guid>
    {
        return DeleteEntityAsync<TEntity, Guid>(model);
    }

    private async Task SaveIfNotInTransactionAsync()
    {
        if (_efTransactionContext.Transactions.Count == 0)
        {
            await SaveChangesAsync(CancellationToken);
        }
    }

    private void Log<TKey>(IEntity<TKey> newObj, LogAction action, IEntity<TKey> oldObj = null)
    {
        LoggerHostInterval.Add(new LogRequest()
        {
            NewObj = newObj,
            OldObj = oldObj,
            Action = action,
            Module = GetType().Name.Replace("DbContext", ""),
            User = UserResolver?.User,
        });
    }

    public override int SaveChanges()
    {
        return SaveChangesAsync(CancellationToken).Result;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return base.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, ex.Message);
            throw;
        }
    }

    public Task MigrateAsync()
    {
        if (GetProvider() != DatabaseProviderType.InMemory)
        {
            return Database.MigrateAsync(CancellationToken);
        }

        return Task.CompletedTask;
    }

    public async Task<List<TEntity>> QueryAsync<TEntity>(string query, Dictionary<string, object> parameters)
    {
        var result = new List<TEntity>();
        await using var command = Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        foreach (var item in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = item.Key;
            parameter.Value = item.Value;
            if (item.Value is Enum) parameter.DbType = DbType.Int32;
            if (item.Value is long) parameter.DbType = DbType.Int32;
            if (item.Value is DateTime) parameter.DbType = DbType.DateTime;
            if (item.Value is null) parameter.Value = DBNull.Value;
            command.Parameters.Add(parameter);
        }

        command.CommandType = CommandType.Text;
        await Database.OpenConnectionAsync(CancellationToken);
        await using var reader = await command.ExecuteReaderAsync(CancellationToken);
        var properties = typeof(TEntity).GetProperties();
        while (await reader.ReadAsync(CancellationToken))
        {
            var columns = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++) columns.Add(reader.GetName(i));
            var transaction = Activator.CreateInstance<TEntity>();
            foreach (var property in properties)
            {
                if (!columns.Contains(property.Name)) continue;
                var value = reader.GetValue(property.Name);
                if (value == DBNull.Value) value = null;
                property.SetValue(transaction, value);
            }

            result.Add(transaction);
        }

        return result;
    }

    public Task BulkInsertEntityAsync<TEntity, TKey>(List<TEntity> items) where TEntity : class, IEntity<TKey>
    {

        if (GetProvider() == DatabaseProviderType.InMemory)
        {
            return AddRangeAsync(items, CancellationToken);
        }

        return this.BulkInsertAsync(items, cancellationToken: CancellationToken);
    }

    public Task BulkInsertEntityAsync<TEntity>(List<TEntity> items) where TEntity : class, IEntity<Guid>
    {
        return BulkInsertEntityAsync<TEntity, Guid>(items);
    }


    public Task BulkUpdateEntityAsync<TEntity, TKey>(List<TEntity> items) where TEntity : class, IEntity<TKey>
    {

        return this.BulkUpdateAsync(items, cancellationToken: CancellationToken);
    }

    public Task BulkUpdateEntityAsync<TEntity>(List<TEntity> items) where TEntity : class, IEntity<Guid>
    {
        return BulkUpdateEntityAsync<TEntity, Guid>(items);
    }

    public Task BulkDeleteEntityAsync<TEntity, TKey>(List<TEntity> items) where TEntity : class, IEntity<TKey>
    {
            return this.BulkDeleteAsync(items, cancellationToken: CancellationToken);
    }

    public Task BulkDeleteEntityAsync<TEntity>(List<TEntity> items) where TEntity : class, IEntity<Guid>
    {
        return BulkDeleteEntityAsync<TEntity, Guid>(items);
    }
}