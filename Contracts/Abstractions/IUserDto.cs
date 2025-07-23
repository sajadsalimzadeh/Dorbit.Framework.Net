using System;
using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.Abstractions;

public interface IUserDto
{
    Guid GetId();
    string GetUsername();
    bool IsActive();
}