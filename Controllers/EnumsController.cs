using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dorbit.Framework.Controllers;

public class EnumsController(IOptions<ConfigProject> configProjects) : BaseController
{
    private static Dictionary<string, Dictionary<int, string>> _items;

    private Dictionary<string, Dictionary<int, string>> GetItems()
    {
        if (_items is null)
        {
            var items = new Dictionary<string, Dictionary<int, string>>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies(configProjects.Value.Namespaces);
            foreach (var enumType in assemblies.SelectMany(x => x.GetTypes().Where(type => type.IsEnum)))
            {
                var enumExporterAttribute = enumType.GetCustomAttribute<EnumExporterAttribute>();
                if(enumExporterAttribute is null) continue;
                
                var enumValues = Enum.GetValues(enumType).Cast<object>();
                items[enumExporterAttribute.Name] = enumValues.ToDictionary(Convert.ToInt32, x => x.ToString());
            }

            _items = items;
        }

        return _items;
    }

    private IActionResult CreateResult(object response)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null // This keeps property names as-is (PascalCase)
        };

        var json = JsonSerializer.Serialize(response.ToQueryResult(), options);
        return Content(json, "application/json");
    }

    [HttpGet]
    [ProducesResponseType(typeof(QueryResult<Dictionary<string, Dictionary<int, string>>>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        return CreateResult(GetItems());
    }
    
    [HttpGet("{name}")]
    [ProducesResponseType(typeof(QueryResult<Dictionary<int, string>>), StatusCodes.Status200OK)]
    public IActionResult Get(string name)
    {
        return CreateResult(GetItems().GetValueOrDefault(name));
    }
}