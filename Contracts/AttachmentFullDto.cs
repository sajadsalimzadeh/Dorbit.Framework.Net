namespace Dorbit.Framework.Contracts;

public class AttachmentFullDto : AttachmentDto
{
    public string Drive { get; set; }
    public string Access { get; set; }
    public bool IsPrivate { get; set; }
}