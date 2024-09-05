using System;

namespace Dorbit.Framework.Attributes;

public class SequenceAttribute(string name) : Attribute
{
    public string Name { get; } = name;
    public string Schema { get; init; } = "public";
    public int StartAt { get; init; } = 1;
    public int IncrementsBy { get; init; } = 1;
}