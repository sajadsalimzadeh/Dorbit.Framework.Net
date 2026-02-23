using System;
using System.Collections.Generic;
using Dorbit.Framework.Contracts.Entities;
using Dorbit.Framework.Utils.Json;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IStatusLog<T> where T : Enum  
{
    public T Status { get; set; }
    
    [JsonField]
    public List<StatusLog<T>> StatusLogs { get; set; }
}