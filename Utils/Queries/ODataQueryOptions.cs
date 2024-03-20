using System;
using Microsoft.AspNetCore.Http;

namespace Dorbit.Framework.Utils.Queries;

public class ODataQueryOptions : QueryOptions
{
    // public HttpRequest Request { get; private set; }
    public string RawValues { get; private set; }
    public bool IsSet { get; set; }

    public int PageSize => (IsSet && Top.Value > 0) ? Top.Value : 10000;
    public int PageIndex => (IsSet && Skip.Value > 0) ? Skip.Value / Top.Value : 0;

    public ODataQueryOptions Parse(HttpRequest request)
    {
        // Request = request; Causes serialization problems over bus
        RawValues = request.QueryString.ToString();
        return Parse();
    }
        
    public ODataQueryOptions Parse(string rawQuery)
    {
        RawValues = rawQuery;
        return Parse();
    }

    public ODataQueryOptions Parse()
    {
        Filters.ODataParseRaw(RawValues);
        OrderBy.ODataParseRaw(RawValues);
        Skip.ODataParseRaw(RawValues);
        Top.ODataParseRaw(RawValues);
        IsSet = true;
            
        return this;
    }
}