using System;

namespace Dorbit.Framework.Contracts.Abstractions;

public interface IServerDto
{
    Guid? GetId();
    string GetName();
}