using System;

namespace Dorbit.Framework.Attributes;

public class EnumExporterAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}