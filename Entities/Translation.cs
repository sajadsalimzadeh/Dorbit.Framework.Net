using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Entities;

[Index(nameof(Key), nameof(Locale))]
public class Translation : Entity
{
    [MaxLength(128)]
    public string Key { get; set; }
    
    [MaxLength(8)]
    public string Locale { get; set; }
    
    [MaxLength(4096)]
    public string Value { get; set; }
}