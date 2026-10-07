using System;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Dorbit.Framework.Contracts.Attachments;

public class AttachmentUploadPrivateRequest
{
    [JsonIgnore]
    public Guid UserId { get; set; }

    public string Name { get; set; }
    public Stream Stream { get; set; }
    public string Access { get; set; }
}