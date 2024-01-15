using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dorbit.Framework.Utils.Json;

public class CustomContractResolver : DefaultContractResolver
{
    private readonly Func<bool> _includeProperty;

    public CustomContractResolver(Func<bool> includeProperty)
    {
        _includeProperty = includeProperty;
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        var shouldSerialize = property.ShouldSerialize;
        property.ShouldSerialize = obj => _includeProperty() &&
                                          (shouldSerialize == null ||
                                           shouldSerialize(obj));
        return property;
    }
}