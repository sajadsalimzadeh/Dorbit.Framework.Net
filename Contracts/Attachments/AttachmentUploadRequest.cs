using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.Attachments;

public class AttachmentUploadRequest
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    [JsonIgnore]
    public Stream Stream { get; set; }

    public string Name { get; set; }
    public string Drive { get; set; }
    public string Description { get; set; }
    public string Access { get; set; }
    public bool IsPrivate { get; set; }
    public List<string> AccessTokens { get; set; }
    public List<Guid> UserIds { get; set; }
}