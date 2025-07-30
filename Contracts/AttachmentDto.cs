using System.ComponentModel.DataAnnotations.Schema;

namespace Dorbit.Framework.Contracts;

[NotMapped]
public class AttachmentDto
{
    public string Name { get; set; }
    public string Filename { get; set; }
    public long Size { get; set; }
}