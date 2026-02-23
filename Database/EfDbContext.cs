using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
using Dorbit.Framework.Services;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Utils.Json;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Dorbit.Framework.Database;

public abstract class EfDbContext : DbContext, IDbContext
{
    private readonly LoggerService _loggerService;
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


    public IServiceProvider ServiceProvider { get; }
    public bool AutoExcludeDeleted { get; set; } = true;

    private DatabaseProviderType? _providerType;

    public DatabaseProviderType ProviderType
    {
        get
        {
            if (_providerType.HasValue) return _providerType.Value;
            var providerName = Database.ProviderName;
            var ignoreCase = StringComparison.InvariantCultureIgnoreCase;
            if (providerName == null) _providerType = DatabaseProviderType.Unknown;
            else if (providerName.Contains("inmemory", ignoreCase)) _providerType = DatabaseProviderType.InMemory;
            else if (providerName.Contains("mysql", ignoreCase)) _providerType = DatabaseProviderType.MySql;
            else if (providerName.Contains("sqlserver", ignoreCase)) _providerType = DatabaseProviderType.SqlServer;
            else if (providerName.Contains("postgresql", ignoreCase)) _providerType = DatabaseProviderType.Postgres;
            else _providerType = DatabaseProviderType.Unknown;
            return _providerType.Value;
        }
    }

    protected EfDbContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options)
    {
        ServiceProvider = serviceProvider;
        _efTransactionContext = new EfTransactionContext(this);
        _loggerService = serviceProvider.GetService<LoggerService>();

        base.ChangeTracker.AutoDetectChangesEnabled = false;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
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

                modelBuilder.HasSequence<int>(sequenceAttribute.Name, schema: sequenceAttribute.Schema)
                    .StartsAt(sequenceAttribute.StartAt)
                    .IncrementsBy(sequenceAttribute.IncrementsBy);

                var propertyBuilder = modelBuilder.Entity(type.ClrType).Property(property.Name);
                propertyBuilder.ValueGeneratedOnAdd();

                if (ProviderType == DatabaseProviderType.Postgres)
                {
                    propertyBuilder.HasDefaultValueSql($"nextval('{sequenceAttribute.Schema}.{sequenceAttribute.Name}')");
                }
            }
        }

        this.ExcludeMigrationByAttribute(modelBuilder);
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
        if (ProviderType == DatabaseProviderType.InMemory) return new InMemoryTransaction(_efTransactionContext);
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

    public async Task<TEntity> InsertEntityAsync<TEntity, TKey>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>
    {
        if (ProviderType == DatabaseProviderType.InMemory)
        {
            var type = typeof(TEntity);
            foreach (var property in type.GetProperties())
            {
                var sequenceAttribute = property.GetCustomAttribute<SequenceAttribute>();
                if (sequenceAttribute is null) continue;

                var key = sequenceAttribute.Schema + "." + sequenceAttribute.Name;
                var counter = SequenceCounter.GetOrAdd(key, _ => sequenceAttribute.StartAt);
                counter += sequenceAttribute.IncrementsBy;
                SequenceCounter[key] = counter;
                property.SetValue(entity, counter);
            }
        }

        entity.ValidateCreation<TEntity, TKey>();
        entity.GenerateKey<TEntity, TKey>();
        entity.GenerateHistoricalId<TEntity, TKey>();

        entity.IncludeCreationAudit<TEntity, TKey>(UserResolver?.User);
        if (entity is IAuditHistory auditHistory) auditHistory.IncludeAudit(LogAction.Insert, UserResolver?.User);
        if (entity is ITenantAudit tenantAudit) tenantAudit.IncludeTenantAudit(TenantResolver?.Tenant);
        if (entity is IServerAudit serverAudit) serverAudit.IncludeServerAudit(ServerResolver?.Server);
        if (entity is ISoftwareAudit softwareAudit) softwareAudit.IncludeSoftwareAudit(SoftwareResolver?.Software);
        if (entity is IChangeLog changeLog) changeLog.IncludeChangeLogs();

        await AddAsync(entity, cancellationToken);
        await SaveIfNotInTransactionAsync(cancellationToken);
        if (entity is ICreationLogging logging) Log(logging, LogAction.Insert);
        return entity;
    }

    public Task<TEntity> InsertEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity<Guid>
    {
        return InsertEntityAsync<TEntity, Guid>(entity, cancellationToken);
    }

    public async Task<TEntity> UpdateEntityAsync<TEntity, TKey>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>
    {
        if (entity is IReadonly) throw new OperationException(FrameworkErrors.EntityIsReadonly);

        entity.ValidateModification<TEntity, TKey>();
        entity.IncludeVersionAudit<TEntity, TKey>();
        entity.IncludeModificationAudit<TEntity, TKey>(UserResolver?.User);

        var readonlyProperties = entity.GetReadonlyProperties<TEntity, TKey>();
        readonlyProperties.ForEach(property => Entry(entity).Property(property.Name).IsModified = false);

        TEntity oldEntity = null;
        if (entity is IHistorical)
        {
            oldEntity = DbSet<TEntity, TKey>().GetById(entity.Id);
            using var transaction = BeginTransaction();
            if (oldEntity is IHistorical oldHistoricalModel)
            {
                oldHistoricalModel.IsHistorical = true;
                Entry(oldHistoricalModel).State = EntityState.Modified;
            }

            if (oldEntity is ICreationTime creationTime) Entry(creationTime).Property(x => x.CreationTime).IsModified = false;
            if (oldEntity is ICreationAudit creationAudit)
            {
                Entry(creationAudit).Property(x => x.CreatorId).IsModified = false;
                Entry(creationAudit).Property(x => x.CreatorName).IsModified = false;
            }

            await InsertEntityAsync<TEntity, TKey>(entity, cancellationToken);
            await transaction.CommitAsync();
        }
        else
        {
            Entry(entity).State = EntityState.Modified;

            if (entity is ICreationTime creationTime) Entry(creationTime).Property(x => x.CreationTime).IsModified = false;
            if (entity is ICreationAudit creationAudit)
            {
                Entry(creationAudit).Property(x => x.CreatorId).IsModified = false;
                Entry(creationAudit).Property(x => x.CreatorName).IsModified = false;
            }
        }

        if (entity is IAuditHistory auditHistory) auditHistory.IncludeAudit(LogAction.Update, UserResolver?.User);
        if (entity is IChangeLog changeLog)
        {
            oldEntity ??= DbSet<TEntity, TKey>().GetById(entity.Id);
            if (oldEntity is IChangeLog oldEntityChangeLog)
            {
                changeLog.IncludeChangeLogs(oldEntityChangeLog);
            }
        }

        await SaveIfNotInTransactionAsync(cancellationToken);
        if (entity is ILogging) Log(entity, LogAction.Update, oldEntity);
        return entity;
    }

    public Task<TEntity> UpdateEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity<Guid>
    {
        return UpdateEntityAsync<TEntity, Guid>(entity, cancellationToken);
    }

    public async Task<TEntity> DeleteEntityAsync<TEntity, TKey>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>
    {
        if (entity is IUnDeletable) throw new OperationException(FrameworkErrors.EntityIsUnDeletable);

        entity.ValidateDeletion<TEntity, TKey>();

        if (entity is ISoftDelete softDelete)
        {
            if (!softDelete.IsDeleted)
            {
                softDelete.IsDeleted = true;
                entity.IncludeDeletionAudit<TEntity, TKey>(UserResolver?.User);
                Entry(softDelete).State = EntityState.Modified;
            }
            else
            {
                Entry(softDelete).State = EntityState.Detached;
            }

            if (entity is IAuditHistory auditHistory) auditHistory.IncludeAudit(LogAction.Delete, UserResolver?.User);
        }
        else
        {
            Entry(entity).State = EntityState.Deleted;
        }

        await SaveIfNotInTransactionAsync(cancellationToken);
        if (entity is ILogging logging) Log(logging, LogAction.Delete);
        return entity;
    }

    public Task<TEntity> DeleteEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity<Guid>
    {
        return DeleteEntityAsync<TEntity, Guid>(entity, cancellationToken);
    }

    private async Task SaveIfNotInTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_efTransactionContext.Transactions.Count == 0)
        {
            await SaveChangesAsync(cancellationToken);
        }
    }

    private void Log<TKey>(IEntity<TKey> newEntity, LogAction action, IEntity<TKey> oldEntity = null)
    {
        _loggerService.Log(new LogEntityRequest()
        {
            NewObj = newEntity,
            OldObj = oldEntity,
            Action = action,
            Module = GetType().Name.Replace("DbContext", ""),
            User = UserResolver?.User,
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            ChangeTracker.Clear();
            return result;
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, ex.Message);
            throw;
        }
    }

    public Task MigrateAsync(CancellationToken cancellationToken)
    {
        if (ProviderType != DatabaseProviderType.InMemory)
        {
            return Database.MigrateAsync(cancellationToken);
        }

        return Task.CompletedTask;
    }

    private DbCommand CreateCommand(string query, Dictionary<string, object> parameters)
    {
        var command = Database.GetDbConnection().CreateCommand();
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
        return command;
    }

    public async Task<int> ExecuteCommandAsync(string query, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        await using var command = CreateCommand(query, parameters);
        await Database.OpenConnectionAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<List<TEntity>> ExecuteQueryAsync<TEntity>(string query, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        var result = new List<TEntity>();
        await using var command = CreateCommand(query, parameters);
        await Database.OpenConnectionAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var properties = typeof(TEntity).GetProperties();
        while (await reader.ReadAsync(cancellationToken))
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

    public async Task BulkInsertEntityAsync<TEntity, TKey>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>
    {
        var user = UserResolver?.User;
        var tenant = TenantResolver?.Tenant;
        var server = ServerResolver?.Server;
        var software = SoftwareResolver?.Software;
        entities.ForEach(entity =>
        {
            entity.ValidateCreation<TEntity, TKey>();
            entity.IncludeCreationAudit<TEntity, TKey>(user);
            entity.GenerateKey<TEntity, TKey>();
            if (entity is ITenantAudit tenantAudit) tenantAudit.IncludeTenantAudit(tenant);
            if (entity is IServerAudit serverAudit) serverAudit.IncludeServerAudit(server);
            if (entity is ISoftwareAudit softwareAudit) softwareAudit.IncludeSoftwareAudit(software);
            if (entity is IChangeLog changeLog) changeLog.IncludeChangeLogs();
        });

        if (ProviderType == DatabaseProviderType.InMemory)
        {
            await AddRangeAsync(entities, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return;
        }

        await this.BulkInsertAsync(entities, cancellationToken: cancellationToken);
    }

    public Task BulkInsertEntityAsync<TEntity>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class, IEntity<Guid>
    {
        return BulkInsertEntityAsync<TEntity, Guid>(entities, cancellationToken);
    }

    public Task BulkUpdateEntityAsync<TEntity, TKey>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>
    {
        var user = UserResolver?.User;
        entities.ForEach(item => item.IncludeModificationAudit<TEntity, TKey>(user));

        if (ProviderType == DatabaseProviderType.InMemory)
        {
            UpdateRange(entities);
            return SaveChangesAsync(cancellationToken);
        }

        return this.BulkUpdateAsync(entities, cancellationToken: cancellationToken);
    }

    public Task BulkUpdateEntityAsync<TEntity>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class, IEntity<Guid>
    {
        return BulkUpdateEntityAsync<TEntity, Guid>(entities, cancellationToken);
    }

    public Task BulkDeleteEntityAsync<TEntity, TKey>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>
    {
        return this.BulkDeleteAsync(entities, cancellationToken: cancellationToken);
    }

    public Task BulkDeleteEntityAsync<TEntity>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class, IEntity<Guid>
    {
        return BulkDeleteEntityAsync<TEntity, Guid>(entities, cancellationToken);
    }

    public Task MigrateAsync()
    {
        if(ProviderType == DatabaseProviderType.InMemory) return Task.CompletedTask;
        return Database.MigrateAsync();
    }
}