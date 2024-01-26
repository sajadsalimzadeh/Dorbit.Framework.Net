using System.Linq;
using System.Reflection;
using Dorbit.Framework.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Dorbit.Framework.Filters;

public class SwaggerExcludeFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties == null) return;

        var ignoreDataMemberProperties = context.Type.GetProperties()
            .Where(t => t.GetCustomAttribute<DocIgnoreAttribute>() != null);

        foreach (var ignoreDataMemberProperty in ignoreDataMemberProperties)
        {
            var propertyToHide = schema.Properties.Keys
                .SingleOrDefault(x => x.ToLower() == ignoreDataMemberProperty.Name.ToLower());

            if (propertyToHide != null) schema.Properties.Remove(propertyToHide);
        }
    }
}