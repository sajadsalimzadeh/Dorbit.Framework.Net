using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Utils.Json;

namespace Dorbit.Framework.Entities;

public class Message : CreateEntity
{
    [JsonField]
    public List<string> Receivers { get; set; }
    [MaxLength(1024)]
    public string Body { get; set; }
    [MaxLength(64)]
    public string TemplateCode { get; set; }
    [MaxLength(1024)]
    public string Description { get; set; }
    public MessageType Type { get; set; }

    [JsonField]
    public List<string> Args { get; set; }
}