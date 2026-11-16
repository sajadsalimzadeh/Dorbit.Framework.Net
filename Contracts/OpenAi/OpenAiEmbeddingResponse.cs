using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.OpenAi;

public class OpenAiEmbeddingResponse
{
    public List<OpenAiEmbeddingResponseData> Data { get; set; }
}

public class OpenAiEmbeddingResponseData
{
    public int Index { get; set; }
    public float[] Embedding { get; set; }
}
