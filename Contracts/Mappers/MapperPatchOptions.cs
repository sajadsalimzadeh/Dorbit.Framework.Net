using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.Mappers;

public class MapperPatchOptions
{
    public HashSet<string> IgnoreProperties { get; set; } = new();
}