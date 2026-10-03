using System;
using System.Text.RegularExpressions;
using Dorbit.Framework.Exceptions;

namespace Dorbit.Framework.Utils.Queries;

internal static class QueryIdentifier
{
    private static readonly Regex MemberPattern = new(
        @"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    public static string RequireMember(string value)
    {
        if (string.IsNullOrEmpty(value) || !MemberPattern.IsMatch(value))
            throw new OperationException(FrameworkErrors.QueryIsInvalid);

        return value;
    }
}
