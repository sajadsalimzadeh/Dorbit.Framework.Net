using System.Reflection;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Extensions;

public static class ODataExtensions
{
    public static IMvcBuilder AddODataDefault(this IMvcBuilder builder, Assembly assembly = null)
    {
        return builder.AddOData(opt => opt.Select().Filter().OrderBy().Count().Expand().SkipToken().SetMaxTop(100));
    }
}