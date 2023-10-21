using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace Dorbit.Utils.Queries
{
    public static class ODataQueryOptionExtensions
    {

        public static FilterQueryOption ODataParse(this FilterQueryOption option, HttpRequest request)
        {
            var queryString = request.Query["$filter"].FirstOrDefault();
            if (request.Query.ContainsKey("$filter")) option.Expression = ODataParser(queryString);
            return option;
        }

        public static FilterQueryOption ODataParseRaw(this FilterQueryOption option, string rawValues)
        {
            rawValues = rawValues.Strip();
            var query = QueryHelpers.ParseQuery(rawValues);
            if (query.ContainsKey("$filter")) option.Expression = ODataParser(query["$filter"]);
            return option;
        }

        private static FilterQueryOptionExpression ODataParser(string query)
        {
            query = query.Trim();
            FilterQueryOptionExpression ex = null;
            ex ??= ParseGroup(query);
            ex ??= ParseLogic(query);
            ex ??= ParseBinary(query);
            ex ??= ParseUnary(query);
            ex ??= ParseLiteral(query);
            return ex;
        }

        private static FilterQueryOptionBinaryExpression ParseBinary(string query)
        {
            if (query?.Length == 0 || (query.First() == '(' && query.Last() == ')')) return null;
            var ex = new FilterQueryOptionBinaryExpression();
            var index = 0;
            foreach (FilterQueryOptionBinaryOperators item in Enum.GetValues(typeof(FilterQueryOptionBinaryOperators)))
            {
                var tmpIndex = query.ToLower().IndexOf(' ' + item.ToString().ToLower() + ' ', StringComparison.Ordinal);
                if (tmpIndex <= index) continue;
                var openPLeft = query.Substring(0, tmpIndex).Count(x => x == '(');
                var closePLeft = query.Substring(0, tmpIndex).Count(x => x == ')');
                var openPRight = query.Substring(tmpIndex).Count(x => x == '(');
                var closePRight = query.Substring(tmpIndex).Count(x => x == ')');

                if (openPLeft != closePLeft || openPRight != closePRight) continue;
                index = tmpIndex;
                ex.Operator = item;
            }

            if (ex.Operator == FilterQueryOptionBinaryOperators.None) return null;
            ex.Left = ODataParser(query.Substring(0, index));
            ex.Right = ODataParser(query.Substring(index + ex.Operator.ToString().Length + 1));
            return ex;
        }

        private static FilterQueryOptionGroupExpression ParseGroup(string query)
        {
            if (query.Length == 0 || query[0] != '(') return null;
            var depth = 1;
            var stringFlag = false;
            var sb = new StringBuilder();
            for (var i = 1; i < query.Length && depth > 0; i++)
            {
                var ch = query[i];
                if (ch == '\'') stringFlag = !stringFlag;
                if (!stringFlag)
                {
                    switch (ch)
                    {
                        case '(':
                            depth++;
                            break;
                        case ')':
                            depth--;
                            break;
                    }
                }

                if (depth > 0) sb.Append(ch);
            }

            var ex = new FilterQueryOptionGroupExpression
            {
                Expression = ODataParser(sb.ToString())
            };
            return ex;
        }

        private static FilterQueryOptionLiteralExpression ParseLiteral(string value)
        {
            if (value.Length == 0) return null;
            var ex = new FilterQueryOptionLiteralExpression();

            if (value[0] == '\'') ex.Value = $"'{value}'";
            else if (DateTime.TryParse(value, out var dateTimeVal)) ex.Value = dateTimeVal;
            else if (value.IndexOf('.') > -1)
            {
                if (float.TryParse(value, out var floatVal)) ex.Value = floatVal;
                else if (double.TryParse(value, out var doubleVal)) ex.Value = doubleVal;
                else if (decimal.TryParse(value, out var decimalVal)) ex.Value = decimalVal;
                ex.Value ??= value;
            }
            else if (bool.TryParse(value, out var boolVal)) ex.Value = boolVal;
            else if (sbyte.TryParse(value, out var sbyteVal)) ex.Value = sbyteVal;
            else if (short.TryParse(value, out var shortVal)) ex.Value = shortVal;
            else if (int.TryParse(value, out var intVal)) ex.Value = intVal;
            else if (long.TryParse(value, out var longVal)) ex.Value = longVal;
            else ex.Value = value;

            return ex;
        }

        private static FilterQueryOptionLogicalExpression ParseLogic(string query)
        {
            if (query?.Length == 0 || (query.First() == '(' && query.Last() == ')')) return null;
            var ex = new FilterQueryOptionLogicalExpression();
            var index = 0;
            foreach (FilterQueryOptionLogicalOperators item in
                Enum.GetValues(typeof(FilterQueryOptionLogicalOperators)))
            {
                var tmpIndex = query.ToLower()
                    .IndexOf(' ' + item.ToString().ToLower() + ' ', StringComparison.Ordinal);
                if (tmpIndex <= index) continue;
                var openPLeft = query.Substring(0, tmpIndex).Count(x => x == '(');
                var closePLeft = query.Substring(0, tmpIndex).Count(x => x == ')');
                var openPRight = query.Substring(tmpIndex).Count(x => x == '(');
                var closePRight = query.Substring(tmpIndex).Count(x => x == ')');

                if (openPLeft != closePLeft || openPRight != closePRight) continue;
                index = tmpIndex;
                ex.Operator = item;
            }

            if (ex.Operator == FilterQueryOptionLogicalOperators.None) return null;
            ex.Left = ODataParser(query.Substring(0, index));
            ex.Right = ODataParser(query.Substring(index + ex.Operator.ToString().Length + 1));
            return ex;
        }

        private static FilterQueryOptionUnaryExpression ParseUnary(string query)
        {
            var ex = new FilterQueryOptionUnaryExpression();
            foreach (FilterQueryOptionUnaryOperators item in Enum.GetValues(typeof(FilterQueryOptionUnaryOperators)))
            {
                if (query.ToLower().IndexOf(item.ToString().ToLower() + ' ', StringComparison.Ordinal) != 0) continue;
                ex.Operator = item;
                break;
            }

            if (ex.Operator == FilterQueryOptionUnaryOperators.None) return null;
            query = query.Substring(ex.Operator.ToString().Length + 1).Trim();
            ex.Expression = ODataParser(query);
            return ex;
        }

        public static OrderByQueryOption ODataParse(this OrderByQueryOption option, HttpRequest request)
        {
            if (!request.Query.ContainsKey("$orderby")) return option;
            var orderBy = request.Query["$orderby"].FirstOrDefault();
            option.Items = new List<KeyValuePair<string, bool>>();
            if (orderBy == null) return option;
            foreach (var item in orderBy.Split(','))
            {
                var itemSplit = item.Trim().Split(' ');
                switch (itemSplit.Length)
                {
                    case 2:
                        option.Items.Add(new KeyValuePair<string, bool>(itemSplit[0],
                            itemSplit[1].ToLower() == "desc"));
                        break;
                    case 1:
                        option.Items.Add(new KeyValuePair<string, bool>(itemSplit[0], false));
                        break;
                }
            }

            return option;
        }

        public static OrderByQueryOption ODataParseRaw(this OrderByQueryOption option, string rawValues)
        {
            rawValues = rawValues.Strip();
            var query = QueryHelpers.ParseQuery(rawValues);

            if (!query.ContainsKey("$orderby")) return option;
            var orderBy = query["$orderby"].FirstOrDefault();
            option.Items = new List<KeyValuePair<string, bool>>();
            if (orderBy == null) return option;
            foreach (var item in orderBy.Split(','))
            {
                var itemSplit = item.Trim().Split(' ');
                switch (itemSplit.Length)
                {
                    case 2:
                        option.Items.Add(new KeyValuePair<string, bool>(itemSplit[0],
                            itemSplit[1].ToLower() == "desc"));
                        break;
                    case 1:
                        option.Items.Add(new KeyValuePair<string, bool>(itemSplit[0], false));
                        break;
                }
            }

            return option;
        }

        public static SkipQueryOption ODataParse(this SkipQueryOption option, HttpRequest request)
        {
            if (!request.Query.ContainsKey("$skip")) return option;
            if (int.TryParse(request.Query["$skip"], out var value)) option.Value = value;

            return option;
        }

        public static SkipQueryOption ODataParseRaw(this SkipQueryOption option, string rawValues)
        {
            rawValues = rawValues.Strip();
            var query = QueryHelpers.ParseQuery(rawValues);

            if (!query.ContainsKey("$skip")) return option;
            if (int.TryParse(query["$skip"], out var value)) option.Value = value;

            return option;
        }

        public static TopQueryOption ODataParse(this TopQueryOption option, HttpRequest request)
        {
            if (!request.Query.ContainsKey("$top")) return option;
            if (int.TryParse(request.Query["$top"], out var value)) option.Value = value;

            return option;
        }

        public static TopQueryOption ODataParseRaw(this TopQueryOption option, string rawValues)
        {
            rawValues = rawValues.Strip();
            var query = QueryHelpers.ParseQuery(rawValues);

            if (!query.ContainsKey("$top")) return option;
            if (int.TryParse(query["$top"], out var value)) option.Value = value;

            return option;
        }

        private static string Strip(this string rawValues)
        {
            if (string.IsNullOrEmpty(rawValues)) return rawValues;
            
            while (rawValues.Count(x => x == '?') > 1)
            {
                var index = rawValues.LastIndexOf("?");
                var sb = new StringBuilder(rawValues) {[index] = '&'};
                rawValues = sb.ToString();
            }
            return rawValues;
        }
    }
}