using System;

namespace Dorbit.Framework.Contracts.Abstractions;

public interface ITenantDto
{
    Guid GetId();
    string GetName();
}