using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Entities;

[Index("Entity", "Key", IsUnique = true, Name = "UN_Lookup_Entity_Key")]
public class Lookup : Entity
{
    [StringLength(64), Required]
    public string Entity { get; set; }
    [StringLength(64), Required]
    public string Key { get; set; }
    public int Value { get; set; }
    [StringLength(128)]
    public string DisplayName { get; set; }
}