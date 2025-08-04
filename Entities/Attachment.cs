
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dorbit.Framework.Utils.Json;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Entities;

[Index(nameof(Filename))]
public class Attachment : CreateEntity
{
    public Guid UserId { get; set; }
    
    [MaxLength(64), Required]
    public string Filename { get; set; }
    public long Size { get; set; }

    [MaxLength(64)]
    public string Access { get; set; }
    public bool IsPrivate { get; set; }

    [JsonField]
    public List<string> AccessTokens { get; set; }
    
    [JsonField]
    public List<Guid> UserIds { get; set; }
}