using System.ComponentModel;
using System.Data;
using System.Reflection;
using AutoMapper;
using Dorbit.Database.Abstractions;
using Dorbit.Entities;
using Dorbit.Entities.Abstractions;
using Dorbit.Enums;
using Dorbit.Exceptions;
using Dorbit.Hosts;
using Dorbit.Repositories;
using Dorbit.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Database
{
    public abstract class EfDbContext : DbContext, IDbContext
    {
        private bool autoExcludeDeleted = true;
        private EfTransactionContext efTransactionContext;
        private readonly List<Type> lookupEntities;
        private readonly IMapper mapper;

        private IUserResolver _userResolver;
        private IUserResolver userResolver => _userResolver ??= ServiceProvider.GetService<IUserResolver>();

        private ITenantResolver _tenantResolver;
        private ITenantResolver tenantResolver => _tenantResolver ??= ServiceProvider.GetService<ITenantResolver>();

        private IServerResolver _serverResolver;
        private IServerResolver serverResolver => _serverResolver ??= ServiceProvider.GetService<IServerResolver>();

        private ISoftwareResolver _softwareResolver;
        private ISoftwareResolver softwareResolver => _softwareResolver ??= ServiceProvider.GetService<ISoftwareResolver>();

        private ILoggerService _loggerService;
        private ILoggerService loggerService => _loggerService ??= ServiceProvider.GetService<ILoggerService>();

        private IMemoryCache _memoryCache;
        private IMemoryCache memoryCache => _memoryCache ??= ServiceProvider.GetService<IMemoryCache>();

        private LoggerHost _loggerHost;
        private LoggerHost loggerHost => _loggerHost ??= ServiceProvider.GetService<LoggerHost>();

        public IServiceProvider ServiceProvider { get; }

        public DbSet<Lookup> Lookups { get; set; }

        public EfDbContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options)
        {
            lookupEntities = new List<Type>();
            mapper = serviceProvider.GetService<IMapper>();
            efTransactionContext = new EfTransactionContext(this);

            ChangeTracker.AutoDetectChangesEnabled = false;
            this.ServiceProvider = serviceProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            RegisterAuditProperties(modelBuilder);

            foreach (var type in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var fkeys in type.GetForeignKeys()
                    .Where(x => !x.IsOwnership && x.DeleteBehavior == DeleteBehavior.Cascade))
                {
                    fkeys.DeleteBehavior = DeleteBehavior.NoAction;
                }
            }

            AddLookupEntity<LogAction>();
        }

        protected void AddLookupEntity<T>() where T : struct, Enum
        {
            lookupEntities.Add(typeof(T));
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
            return lookupEntities.ToList();
        }

        public ITransaction BeginTransaction()
        {
            return efTransactionContext.BeginTransaction();
        }

        public IDbContext AutoExcludeDeletedEnable()
        {
            autoExcludeDeleted = true;
            return this;
        }

        public IDbContext AutoExcludeDeletedDisable()
        {
            autoExcludeDeleted = false;
            return this;
        }

        public IQueryable<T> DbSet<T>() where T : class, IEntity
        {
            return DbSet<T>(autoExcludeDeleted);
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

        public T InsertEntity<T>(T model) where T : class, IEntity
        {
            var e = new ModelValidationException();
            if (model is IValidator validator) validator.Validate(e, ServiceProvider);
            if (model is ICreationValidator creationValidator) creationValidator.ValidateOnCreate(e, ServiceProvider);
            e.ThrowIfHasError();
            if (model is ICreationTime creationTime) creationTime.CreationTime = DateTime.Now;
            if (model is ICreationAudit creationAudit)
            {
                var user = userResolver?.GetUser();
                creationAudit.CreatorId = user?.Id;
                creationAudit.CreatorName = user?.Name;
            }
            if (model is ITenantAudit tenantAudit)
            {
                var tenant = tenantResolver?.GetTenant();
                tenantAudit.TenantId = tenant?.Id;
                tenantAudit.TenantName = tenant?.Name;
            }
            if (model is IServerAudit serverAudit)
            {
                var server = serverResolver?.GetServer();
                serverAudit.ServerId = server?.Id;
                serverAudit.ServerName = server?.Name;
            }
            if (model is ISoftwareAudit softwareAudit)
            {
                var software = softwareResolver?.GetSoftware();
                softwareAudit.SoftwareId = software?.Id;
                softwareAudit.SoftwareName = software?.Name;
            }
            if (model is IHistorical historical)
            {
                historical.IsHistorical = false;
                historical.HistoryId = Guid.NewGuid();
            }
            Entry(model).State = EntityState.Added;
            SaveIfNotInTransaction();
            if (model is ICreationLogging logging) Log(logging, LogAction.Insert);
            return model;
        }

        public T UpdateEntity<T>(T model) where T : class, IEntity
        {
            if (model is IReadonly) throw new OperationException(Errors.EntityIsReadonly);

            var e = new ModelValidationException();
            if (model is IValidator validator) validator.Validate(e, ServiceProvider);
            if (model is IModificationValidator modificationValidator) modificationValidator.ValidateOnModify(e, ServiceProvider);
            e.ThrowIfHasError();

            if (model is IVersionAudit versionAudit) versionAudit.Version++;
            if (model is IModificationTime modificationTime) modificationTime.ModificationTime = DateTime.Now;
            if (model is IModificationAudit modificationAudit)
            {
                var user = userResolver?.GetUser();
                modificationAudit.ModifierId = user?.Id;
                modificationAudit.ModifierName = user?.Name;
            }
            UnTrackhUnChangeEntries<T>(model.Id);
            var oldModel = DbSet<T>().FirstOrDefault(x => x.Id == model.Id);
            if (model is IHistorical historical)
            {
                using var transaction = BeginTransaction();
                var oldHistoricalModel = oldModel as IHistorical;
                oldHistoricalModel.IsHistorical = true;
                Entry(oldHistoricalModel).State = EntityState.Modified;
                if (oldModel is ICreationTime creationTime) Entry(creationTime).Property(x => x.CreationTime).IsModified = false;
                if (oldModel is ICreationAudit creationAudit)
                {
                    Entry(creationAudit).Property(x => x.CreatorId).IsModified = false;
                    Entry(creationAudit).Property(x => x.CreatorName).IsModified = false;
                }
                historical.Id = 0;
                InsertEntity(historical);
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
            SaveIfNotInTransaction();
            if (model is ILogging logging) Log(logging, LogAction.Update, oldModel);
            return model;
        }

        public T RemoveEntity<T>(T model) where T : class, IEntity
        {
            if (model is IUnDeletable) throw new OperationException(Errors.EntityIsUnDeletable);

            var e = new ModelValidationException();
            if (model is IValidator validator) validator.Validate(e, ServiceProvider);
            if (model is IDeletationValidator deletationValidator) deletationValidator.ValidateOnDelete(e, ServiceProvider);
            e.ThrowIfHasError();

            UnTrackhUnChangeEntries<T>(model.Id);
            if (model is ISoftDelete)
            {
                var softDelete = DbSet<T>(false).GetById(model.Id) as ISoftDelete;
                if (!softDelete.IsDeleted)
                {
                    softDelete.IsDeleted = true;

                    if (softDelete is IDeletationTime deletationTime) deletationTime.DeletationTime = DateTime.Now;
                    if (softDelete is IDeletationAudit deletationAudit)
                    {
                        var user = userResolver?.GetUser();
                        deletationAudit.DeleterId = user?.Id;
                        deletationAudit.DeleterName = user?.Name;
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
            SaveIfNotInTransaction();
            if (model is ILogging logging) Log(logging, LogAction.Delete);
            return model;
        }

        private void SaveIfNotInTransaction()
        {
            if (efTransactionContext.Transactions.Count == 0)
            {
                SaveChanges();
            }
        }

        private void UnTrackhUnChangeEntries<T>(long id) where T : class, IEntity
        {
            ChangeTracker.Entries<T>()
                .Where(x => x.State == EntityState.Unchanged && x.Entity.Id == id)
                .ToList()
                .ForEach(x =>
                {
                    x.State = EntityState.Detached;
                });
        }

        public void UnTrackhUnChangeEntries<T>(T model) where T : class, IEntity
        {
            UnTrackhUnChangeEntries<T>(model.Id);
        }

        private void Log(IEntity newObj, LogAction action, IEntity oldObj = null)
        {
            loggerHost.Add(new Models.Loggers.LogRequest()
            {
                NewObj = newObj,
                OldObj = oldObj,
                Action = action,
                Module = GetType().Name.Replace("DbContext", ""),
                User = userResolver?.GetUser(),
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
                loggerService.LogError(ex);
                throw;
            }
        }

        public void Migrate()
        {
            Database.Migrate();
        }

        public List<T> Query<T>(string query, Dictionary<string, object> parameters)
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
            using var reader = command.ExecuteReader();
            var properties = typeof(T).GetProperties();
            while (reader.Read())
            {
                var columns = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++) columns.Add(reader.GetName(i));
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
}
