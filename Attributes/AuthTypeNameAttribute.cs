using System;

namespace Dorbit.Framework.Attributes;

public class AuthTypeNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}