using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Dorbit.Framework.Utils.Json;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IChangeLog
{
    [JsonField]
    public List<ChangeLog> ChangeLogs { get; set; }
}

[NotMapped]
public class ChangeLog
{
    public DateTime Timestamp { get; set; }
    public string Property { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
}