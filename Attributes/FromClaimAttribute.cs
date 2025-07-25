using System;

namespace Dorbit.Framework.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromClaimAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}