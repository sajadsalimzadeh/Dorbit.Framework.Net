using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dorbit.Framework.Extensions;

public static class PropertyBuilderExtensions
{
    public static void Json<T>(this PropertyBuilder<T> builder)
    {
        builder.HasConversion(
            x => JsonSerializer.Serialize(x, JsonSerializerOptions.Default),
            x => JsonSerializer.Deserialize<T>(x, JsonSerializerOptions.Default)
        );
    }
}