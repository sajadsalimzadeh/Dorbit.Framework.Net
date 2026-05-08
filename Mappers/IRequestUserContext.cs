using System;
using System.Text.Json.Serialization;

namespace Dorbit.Framework.Mappers;

public interface IRequestUserContext
{
    [JsonIgnore]
    public Guid RequestUserId { get; set; }
}