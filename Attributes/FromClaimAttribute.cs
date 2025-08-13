using System;

namespace Dorbit.Framework.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromClaimAttribute(string type) : Attribute
{
    public string Type { get; } = type;
}