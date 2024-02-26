using System;

namespace Dorbit.Framework.Attributes;

public class SequenceAttribute(string name) : Attribute
{
    public string Name { get; } = name;
    public int StartAt { get; set; } = 0;
    public int IncrementsBy { get; set; } = 1;
}