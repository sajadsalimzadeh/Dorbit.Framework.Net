using System;

namespace Dorbit.Framework.Contracts.Abstractions;

public interface ISoftwareDto
{
    Guid? GetId();
    string GetName();
}