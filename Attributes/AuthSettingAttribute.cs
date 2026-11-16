using System;

namespace Dorbit.Framework.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
public class AuthSettingAttribute(string access = null) : Attribute
{
    public string Access { get; } = access;
}