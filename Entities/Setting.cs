using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Entities;

[Index(nameof(Key), IsUnique = true)]
public class Setting : Entity
{
    [MaxLength(32), Required]
    public string Key { get; set; }
    [MaxLength(10240)]
    public string Value { get; set; }

    [MaxLength(64)]
    public string Access { get; set; }
}