using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Mobicar.Core.WebApi.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Dorbit.Framework.Installers;

public static class SwaggerInstaller
{
    public static void UseDefaultOptions(this SwaggerUIOptions options, string title, string version = "v1")
    {
        options.DocExpansion(DocExpansion.None);
        options.EnableFilter();
        options.SwaggerEndpoint($"/swagger/{version}/swagger.json", title);
    }

    public static void UseDefaultOptions(this SwaggerGenOptions options, string title, string version = "v1", Uri uri = null)
    {
        options.CustomSchemaIds(type => type.FullName);
        options.SwaggerDoc(version, new OpenApiInfo
            {
                Version = version,
                Title = title,
                Description = $"{title} APIs",
                Contact = new OpenApiContact
                {
                    Name = $"{title} Backend Development Team",
                    Url = uri
                }
            }
        );

        var xmlFilename = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
        if (File.Exists(xmlFilename))
        {
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        }

        options.SchemaFilter<SwaggerExcludeFilter>();
    }

    public static void UseSecurityOptions(this SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Place your token here",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
}