using System;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IDeletionAudit : IDeletionTime, ISoftDelete
{
    string DeleterId { get; set; }
    string DeleterName { get; set; }
}