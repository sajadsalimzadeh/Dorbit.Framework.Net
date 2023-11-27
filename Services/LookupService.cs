using System.ComponentModel;
using System.Reflection;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Services.Abstractions;

namespace Dorbit.Framework.Services;

[ServiceRegister]
internal class LookupService : ILookupService
{
    private readonly IEnumerable<IDbContext> dbContexts;

    public LookupService(IEnumerable<IDbContext> dbContexts)
    {
        this.dbContexts = dbContexts;
    }

    public void Initiate()
    {
        foreach (var dbContext in dbContexts)
        {
            using var transaction = dbContext.BeginTransaction();
            foreach (var type in dbContext.GetLookupEntities())
            {
                foreach (var value in Enum.GetValues(type))
                {
                    var name = Enum.GetName(type, value);
                    var enumValueMemberInfo = type.GetMember(name).FirstOrDefault(m => m.DeclaringType == type);
                    var descriptionAttr = enumValueMemberInfo.GetCustomAttribute<DescriptionAttribute>();
                    var rec = dbContext.DbSet<Lookup>().FirstOrDefault(x => x.Entity == type.Name && x.Key == name);
                    if (rec is null)
                    {
                            
                        dbContext.InsertEntityAsync(new Lookup()
                        {
                            Entity = type.Name,
                            Key = name,
                            Value = Convert.ToInt32(value),
                            DisplayName = descriptionAttr?.Description
                        });
                    }
                    else if(rec.DisplayName != descriptionAttr?.Description)
                    {
                        rec.DisplayName = descriptionAttr?.Description;
                        dbContext.UpdateEntityAsync(rec);
                    }
                }
            }
            transaction.Commit();
        }
    }
}