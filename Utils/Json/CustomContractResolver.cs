using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dorbit.Framework.Utils.Json;

public class CustomContractResolver(Func<bool> includeProperty) : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        var shouldSerialize = property.ShouldSerialize;
        property.ShouldSerialize = obj => includeProperty() &&
                                          (shouldSerialize == null ||
                                           shouldSerialize(obj));
        return property;
    }
}