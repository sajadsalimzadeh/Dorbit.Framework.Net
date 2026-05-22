using System;

namespace Dorbit.Framework.Attributes;

public class TypeNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}