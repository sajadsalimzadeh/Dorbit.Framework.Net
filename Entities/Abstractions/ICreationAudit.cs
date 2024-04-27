using System;

namespace Dorbit.Framework.Entities.Abstractions;

public interface ICreationAudit : ICreationTime
{
    string CreatorId { get; set; }
    string CreatorName { get; set; }
}