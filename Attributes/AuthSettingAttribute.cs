using System;

namespace Dorbit.Framework.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
public class AuthSettingAttribute(string access = "") : Attribute
{
    public string Access { get; } = access;
}