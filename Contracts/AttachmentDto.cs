using System.ComponentModel.DataAnnotations.Schema;

namespace Dorbit.Framework.Contracts;

[NotMapped]
public class AttachmentDto
{
    public string Filename { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}