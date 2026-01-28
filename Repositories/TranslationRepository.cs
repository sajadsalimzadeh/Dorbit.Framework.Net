using Dorbit.Framework.Attributes;
using Dorbit.Framework.Database;
using Dorbit.Framework.Entities;

namespace Dorbit.Framework.Repositories;

[ServiceRegister]
public class TranslationRepository(FrameworkDbContext dbContext) : BaseRepository<Translation>(dbContext);