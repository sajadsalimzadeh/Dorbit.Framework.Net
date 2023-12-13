using System.ComponentModel;
using System.Data;
using System.Reflection;
using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Enums;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Hosts;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Dorbit.Framework.Database;

public abstract class EfDbContext : DbContext, IDbContext
{
    private bool _autoExcludeDeleted = true;
    private EfTransactionContext _efTransactionContext;
    private readonly List<Type> _lookupEntities;

    private IUserResolver _userResolver;
    private IUserResolver UserResolver => _userResolver ??= ServiceProvider.GetService<IUserResolver>();

    private ITenantResolver _tenantResolver;
    private ITenantResolver TenantResolver => _tenantResolver ??= ServiceProvider.GetService<ITenantResolver>();

    private IServerResolver _serverResolver;
    private IServerResolver ServerResolver => _serverResolver ??= ServiceProvider.GetService<IServerResolver>();

    private ISoftwareResolver _softwareResolver;
    private ISoftwareResolver SoftwareResolver => _softwareResolver ??= ServiceProvider.GetService<ISoftwareResolver>();

    private ILoggerService _loggerService;
    private ILoggerService LoggerService => _loggerService ??= ServiceProvider.GetService<ILoggerService>();

    private LoggerHost _loggerHost;
    private LoggerHost LoggerHost => _loggerHost ??= ServiceProvider.GetService<LoggerHost>();

    public IServiceProvider ServiceProvider { get; }

    public DbSet<Lookup> Lookups { get; set; }

    public EfDbContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options)
    {
        _lookupEntities = new List<Type>();
        _efTransactionContext = new EfTransactionContext(this);
        ServiceProvider = serviceProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        RegisterAuditProperties(modelBuilder);

        foreach (var type in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var keys in type.GetForeignKeys()
                         .Where(x => !x.IsOwnership && x.DeleteBehavior == DeleteBehavior.Cascade))
            {
                keys.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }

        AddLookupEntity<LogAction>();
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

    protected void AddLookupEntity<T>() where T : struct, Enum
    {
        _lookupEntities.Add(typeof(T));
    }

    private void RegisterAuditProperties(ModelBuilder modelBuilder)
    {
        foreach (var type in modelBuilder.Model.GetEntityTypes().Select(x => x.ClrType))
        {
            if (typeof(ICreationAudit).IsAssignableFrom(type))
                modelBuilder.Entity(type).Property(nameof(ICreationAudit.CreatorName)).HasMaxLength(256);

            if (typeof(IModificationAudit).IsAssignableFrom(type))
                modelBuilder.Entity(type).Property(nameof(IModificationAudit.ModifierName)).HasMaxLength(256);

            if (typeof(IDeletationAudit).IsAssignableFrom(type))
                modelBuilder.Entity(type).Property(nameof(IDeletationAudit.DeleterName)).HasMaxLength(256);

            if (typeof(ITenantAudit).IsAssignableFrom(type))
                modelBuilder.Entity(type).Property(nameof(ITenantAudit.TenantName)).HasMaxLength(256);

            if (typeof(IServerAudit).IsAssignableFrom(type))
                modelBuilder.Entity(type).Property(nameof(IServerAudit.ServerName)).HasMaxLength(256);

            if (typeof(ISoftwareAudit).IsAssignableFrom(type))
                modelBuilder.Entity(type).Property(nameof(ISoftwareAudit.SoftwareName)).HasMaxLength(256);
        }
    }

    public IEnumerable<Type> GetLookupEntities()
    {
        return _lookupEntities.ToList();
    }

    public ITransaction BeginTransaction()
    {
        if (GetProvider() == DatabaseProviderType.InMemory) return new InMemoryTransaction();
        return _efTransactionContext.BeginTransaction();
    }

    public IDbContext AutoExcludeDeletedEnable()
    {
        _autoExcludeDeleted = true;
        return this;
    }

    public IDbContext AutoExcludeDeletedDisable()
    {
        _autoExcludeDeleted = false;
        return this;
    }

    public IQueryable<T> DbSet<T>() where T : class, IEntity
    {
        return DbSet<T>(_autoExcludeDeleted);
    }

    public IQueryable<T> DbSet<T>(bool excludeDeleted) where T : class, IEntity
    {
        IQueryable<T> set = Set<T>().AsNoTracking();
        if (excludeDeleted)
        {
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                set = set.Cast<ISoftDelete>().Where(x => !x.IsDeleted).Cast<T>().AsQueryable();
            }
        }
        return set;
    }

    public async Task<T> InsertEntityAsync<T>(T model) where T : class, IEntity
    {
        var e = new ModelValidationException();
        if (model is IValidator validator) validator.Validate(e, ServiceProvider);
        if (model is ICreationValidator creationValidator) creationValidator.ValidateOnCreate(e, ServiceProvider);
        e.ThrowIfHasError();
        if (model is ICreationTime creationTime) creationTime.CreationTime = DateTime.UtcNow;
        if (model is ICreationAudit creationAudit)
        {
            var user = UserResolver?.User;
            creationAudit.CreatorId = user?.Id;
            creationAudit.CreatorName = user?.Name;
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

        if(model.Id == Guid.Empty) model.Id = Guid.NewGuid();
        await AddAsync(model);
        await SaveIfNotInTransactionAsync();
        if (model is ICreationLogging logging) Log(logging, LogAction.Insert);
        return model;
    }

    public async Task<T> UpdateEntityAsync<T>(T model) where T : class, IEntity
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
            modificationAudit.ModifierId = user?.Id;
            modificationAudit.ModifierName = user?.Name;
        }
        var oldModel = DbSet<T>().FirstOrDefault(x => x.Id == model.Id);
        if (model is IHistorical historical)
        {
            using var transaction = BeginTransaction();
            var oldHistoricalModel = oldModel as IHistorical;
            if (oldHistoricalModel != null)
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
            await InsertEntityAsync(historical);
            transaction.Commit();
        }
        else
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
        await SaveIfNotInTransactionAsync();
        if (model is ILogging logging) Log(logging, LogAction.Update, oldModel);
        return model;
    }

    public async Task<T> RemoveEntityAsync<T>(T model) where T : class, IEntity
    {
        if (model is IUnDeletable) throw new OperationException(Errors.EntityIsUnDeletable);

        var e = new ModelValidationException();
        if (model is IValidator validator) validator.Validate(e, ServiceProvider);
        if (model is IDeletationValidator deletionValidator) deletionValidator.ValidateOnDelete(e, ServiceProvider);
        e.ThrowIfHasError();

        if (model is ISoftDelete)
        {
            var softDelete = DbSet<T>(false).GetById(model.Id) as ISoftDelete;
            if (softDelete != null && softDelete.IsDeleted)
            {
                softDelete.IsDeleted = true;

                if (softDelete is IDeletationTime deletionTime) deletionTime.DeletionTime = DateTime.UtcNow;
                if (softDelete is IDeletationAudit deletionAudit)
                {
                    var user = UserResolver?.User;
                    deletionAudit.DeleterId = user?.Id;
                    deletionAudit.DeleterName = user?.Name;
                }

                Entry(softDelete).State = EntityState.Detached;
                Entry(softDelete).State = EntityState.Modified;
            }
            else
            {
                Entry(model).State = EntityState.Deleted;
            }
        }
        else
        {
            Entry(model).State = EntityState.Deleted;
        }
        await SaveIfNotInTransactionAsync();
        if (model is ILogging logging) Log(logging, LogAction.Delete);
        return model;
    }

    private async Task SaveIfNotInTransactionAsync()
    {
        if (_efTransactionContext.Transactions.Count == 0)
        {
            await SaveChangesAsync();
        }
    }

    private void Log(IEntity newObj, LogAction action, IEntity oldObj = null)
    {
        LoggerHost.Add(new Models.Loggers.LogRequest()
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
        try
        {
            return base.SaveChanges();
        }
        catch (Exception ex)
        {
            LoggerService.LogError(ex);
            throw;
        }
    }

    public Task MigrateAsync()
    {
        return Database.MigrateAsync();
    }

    public async Task<List<T>> QueryAsync<T>(string query, Dictionary<string, object> parameters)
    {
        var result = new List<T>();
        using var command = Database.GetDbConnection().CreateCommand();
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
        Database.OpenConnection();
        using var reader = await command.ExecuteReaderAsync();
        var properties = typeof(T).GetProperties();
        while (await reader.ReadAsync())
        {
            var columns = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++) columns.Add(reader.GetName(i));
            var transaction = Activator.CreateInstance<T>();
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
}