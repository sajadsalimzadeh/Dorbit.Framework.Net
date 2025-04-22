using System.IO;
using Newtonsoft.Json;

namespace Dorbit.Framework.Utils.Json;

public class CustomJsonTextWriter(TextWriter textWriter) : JsonTextWriter(textWriter)
{
    public int CurrentDepth { get; private set; }

    public override void WriteStartObject()
    {
        CurrentDepth++;
        base.WriteStartObject();
    }

    public override void WriteEndObject()
    {
        CurrentDepth--;
        base.WriteEndObject();
    }
}