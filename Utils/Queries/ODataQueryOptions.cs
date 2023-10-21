using Microsoft.AspNetCore.Http;

namespace Dorbit.Utils.Queries
{ 
    public class ODataQueryOptions : QueryOptions
    {
        // public HttpRequest Request { get; private set; }
        public string RawValues { get; private set; }
        public bool IsSet { get; set; }

        public int PageSize =>
            (IsSet && Switches.EnableTop && Top.Value > 0)
                ? Top.Value
                : Defaults.PageSize;

        public int PageIndex =>
            (IsSet && Switches.EnableSkip && Skip.Value > 0)
                ? Skip.Value / Top.Value
                : Defaults.PageIndex;

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

        public ODataQueryOptions Patch(
            Action<QueryOptionsSwitches> switches = default,
            Action<QueryOptionsDefaults> defaults = default,
            Action<QueryOptionsPatches> patches = default
        )
        {
            defaults?.Invoke(Defaults);
            switches?.Invoke(Switches);
            patches?.Invoke(Patches);

            if (Patches.BypassPagination)
            {
                Switches.EnableSkip = false;
                Switches.EnableTop = false;
                Defaults.PageIndex = 0;
                Defaults.PageSize = int.MaxValue;
            }

            return this;
        }
    }
}