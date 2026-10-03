using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dorbit.Framework.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Dorbit.Framework.Utils.Queries;

public class ODataQueryOptions : QueryOptions
{
    // public HttpRequest Request { get; private set; }
    public string Query { get; }

    private ODataQueryOptions(string query)
    {
        Query = query;
    }

    public static ODataQueryOptions Parse(HttpRequest request)
    {
        return Parse(request.QueryString.ToString());
    }

    public static ODataQueryOptions Parse(string query)
    {
        if (query?.Length > 8000)
            throw new OperationException(FrameworkErrors.QueryIsTooComplex);

        var queryOptions = new ODataQueryOptions(query);
        var queryDictionary = QueryHelpers.ParseQuery(Strip(query));
        queryOptions.ParseFilters(queryDictionary);
        queryOptions.ParseOrderBy(queryDictionary);
        queryOptions.ParseSkip(queryDictionary);
        queryOptions.ParseTake(queryDictionary);
        return queryOptions;
    }

    private void ParseFilters(Dictionary<string, StringValues> query)
    {
        if (query.TryGetValue("$filter", out var value))
        {
            Filters.Expression = ParseFilters(value);
        }
    }

    private static FilterQueryOptionExpression ParseFilters(string query)
    {
        if (query.Length > 4000)
            throw new OperationException(FrameworkErrors.QueryIsTooComplex);

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
            var openPLeft = query[..tmpIndex].Count(x => x == '(');
            var closePLeft = query[..tmpIndex].Count(x => x == ')');
            var openPRight = query[tmpIndex..].Count(x => x == '(');
            var closePRight = query[tmpIndex..].Count(x => x == ')');

            if (openPLeft != closePLeft || openPRight != closePRight) continue;
            index = tmpIndex;
            ex.Operator = item;
        }

        if (ex.Operator == FilterQueryOptionBinaryOperators.None) return null;
        ex.Left = ParseFilters(query[..index]);
        ex.Right = ParseFilters(query[(index + ex.Operator.ToString().Length + 1)..]);
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
            Expression = ParseFilters(sb.ToString())
        };
        return ex;
    }

    private static FilterQueryOptionLiteralExpression ParseLiteral(string value)
    {
        if (value.Length == 0) return null;
        var ex = new FilterQueryOptionLiteralExpression();

        if (value.Length >= 2 && value[0] == '\'' && value[^1] == '\'')
        {
            ex.IsConstant = true;
            ex.Value = value[1..^1].Replace("''", "'");
        }
        else if (DateTime.TryParse(value, out var dateTimeVal))
        {
            ex.IsConstant = true;
            ex.Value = dateTimeVal;
        }
        else if (value.IndexOf('.') > -1)
        {
            ex.IsConstant = true;
            if (float.TryParse(value, out var floatVal)) ex.Value = floatVal;
            else if (double.TryParse(value, out var doubleVal)) ex.Value = doubleVal;
            else if (decimal.TryParse(value, out var decimalVal)) ex.Value = decimalVal;
            else
            {
                ex.IsConstant = false;
                ex.Value = value;
            }
        }
        else if (bool.TryParse(value, out var boolVal))
        {
            ex.IsConstant = true;
            ex.Value = boolVal;
        }
        else if (sbyte.TryParse(value, out var sbyteVal))
        {
            ex.IsConstant = true;
            ex.Value = sbyteVal;
        }
        else if (short.TryParse(value, out var shortVal))
        {
            ex.IsConstant = true;
            ex.Value = shortVal;
        }
        else if (int.TryParse(value, out var intVal))
        {
            ex.IsConstant = true;
            ex.Value = intVal;
        }
        else if (long.TryParse(value, out var longVal))
        {
            ex.IsConstant = true;
            ex.Value = longVal;
        }
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
            var openPLeft = query[..tmpIndex].Count(x => x == '(');
            var closePLeft = query[..tmpIndex].Count(x => x == ')');
            var openPRight = query[tmpIndex..].Count(x => x == '(');
            var closePRight = query[tmpIndex..].Count(x => x == ')');

            if (openPLeft != closePLeft || openPRight != closePRight) continue;
            index = tmpIndex;
            ex.Operator = item;
        }

        if (ex.Operator == FilterQueryOptionLogicalOperators.None) return null;
        ex.Left = ParseFilters(query[..index]);
        ex.Right = ParseFilters(query[(index + ex.Operator.ToString().Length + 1)..]);
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
        query = query[(ex.Operator.ToString().Length + 1)..].Trim();
        ex.Expression = ParseFilters(query);
        return ex;
    }

    private OrderByQueryOption ODataParse(OrderByQueryOption option, HttpRequest request)
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

    private void ParseOrderBy(Dictionary<string, StringValues> query)
    {
        if (!query.TryGetValue("$orderby", out var value)) return;
        var orderBy = value.FirstOrDefault();
        OrderBy.Items = [];
        if (orderBy == null) return;
        var items = orderBy.Split(',');
        if (items.Length > 8)
            throw new OperationException(FrameworkErrors.QueryIsTooComplex);

        foreach (var item in items)
        {
            var itemSplit = item.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            switch (itemSplit.Length)
            {
                case 2:
                    QueryIdentifier.RequireMember(itemSplit[0]);
                    OrderBy.Items.Add(new KeyValuePair<string, bool>(itemSplit[0],
                        itemSplit[1].ToLower() == "desc"));
                    break;
                case 1:
                    QueryIdentifier.RequireMember(itemSplit[0]);
                    OrderBy.Items.Add(new KeyValuePair<string, bool>(itemSplit[0], false));
                    break;
                default:
                    throw new OperationException(FrameworkErrors.QueryIsInvalid);
            }
        }
    }

    private SkipQueryOption ODataParse(SkipQueryOption option, HttpRequest request)
    {
        if (!request.Query.ContainsKey("$skip")) return option;
        if (int.TryParse(request.Query["$skip"], out var value)) option.Value = value;

        return option;
    }

    private void ParseSkip(Dictionary<string, StringValues> query)
    {
        if (query.TryGetValue("$skip", out var value))
        {
            if (int.TryParse(value, out var skip)) Skip.Value = Math.Max(0, skip);
        }
    }

    private void ParseTake(Dictionary<string, StringValues> query)
    {
        if (query.TryGetValue("$top", out var topStr))
        {
            if (int.TryParse(topStr, out var take)) Top.Value = NormalizeTake(take);
        }
        else if (query.TryGetValue("$take", out var takeStr))
        {
            if (int.TryParse(takeStr, out var take)) Top.Value = NormalizeTake(take);
        }
    }

    private static int NormalizeTake(int take)
    {
        if (take < 0) return 0;
        return Math.Min(take, QueryOptions.MaxTake);
    }

    private static string Strip(string rawValues)
    {
        if (string.IsNullOrEmpty(rawValues)) return rawValues;

        while (rawValues.Count(x => x == '?') > 1)
        {
            var index = rawValues.LastIndexOf("?", StringComparison.Ordinal);
            var sb = new StringBuilder(rawValues) { [index] = '&' };
            rawValues = sb.ToString();
        }

        return rawValues;
    }
}