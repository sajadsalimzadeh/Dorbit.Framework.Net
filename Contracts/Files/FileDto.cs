using System;
using Dorbit.Framework.Entities;

namespace Dorbit.Framework.Contracts.Files;

public class FileDto
{
    public Attachment Attachment { get; set; }
    public DateTimeOffset? LastModifyTime { get; set; }
    public byte[] Content { get; set; }
}