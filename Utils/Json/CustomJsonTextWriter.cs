using Newtonsoft.Json;

namespace Dorbit.Framework.Utils.Json;

public class CustomJsonTextWriter : JsonTextWriter
{
    public CustomJsonTextWriter(TextWriter textWriter) : base(textWriter) { }

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