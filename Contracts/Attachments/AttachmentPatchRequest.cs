using System;
using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.Attachments;

public class AttachmentPatchRequest
{
    public string Access { get; set; }
    public List<string> AccessTokens { get; set; }
    public List<Guid> UserIds { get; set; }
}