using System;

namespace Dorbit.Framework.Entities.Abstractions;

public interface ICreationAudit : ICreationTime
{
    Guid? CreatorId { get; set; }
    string CreatorName { get; set; }
}