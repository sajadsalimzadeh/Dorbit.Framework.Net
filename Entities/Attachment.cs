
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dorbit.Framework.Utils.Json;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Entities;

[Index(nameof(Filename))]
[Index(nameof(Drive), nameof(Filename))]
public class Attachment : CreateEntity
{
    public Guid UserId { get; set; }
    
    [MaxLength(64)]
    public string Drive { get; set; }
    
    [MaxLength(128)]
    public string Name { get; set; }
    
    [MaxLength(512)]
    public string Description { get; set; }
    
    [MaxLength(128), Required]
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