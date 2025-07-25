﻿using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Identities;

namespace Dorbit.Framework.Services.Abstractions;

public interface IIdentityService
{
    IdentityDto Identity { get; }
    Task<IdentityDto> ValidateAsync(IdentityValidateRequest request);
}