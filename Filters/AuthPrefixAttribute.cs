using System;

namespace Dorbit.Framework.Filters;

public class AuthPrefixAttribute(string prefix) : Attribute
{
    public string Prefix { get; } = prefix;
}