using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Jira;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Utils.Http;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class JiraService(IServiceProvider serviceProvider) : HttpClientApi<ConfigJira>(serviceProvider)
{
    private static readonly Regex IssueKeyPattern = new("^[A-Za-z][A-Za-z0-9_]+-\\d+$", RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    private static readonly Regex FieldPattern = new("^[A-Za-z][A-Za-z0-9_]*$", RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    private static readonly Regex TransitionIdPattern = new("^\\d+$", RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    protected override HttpHelper GetHttpHelper()
    {
        var http = GetHttpHelperWithoutClientInfo();
        http.AddHeader("Accept", "application/json");

        http.IsAcceptGzipResponse = false;
        http.AuthorizationToken = Config.ApiKey.GetDecryptedValue();

        return http;
    }

    public Task<HttpModel<JiraCreateNewIssueResponse>> CreateNewIssueAsync(JiraCreateNewIssueRequest request)
    {
        return GetHttpHelper().PostAsync<JiraCreateNewIssueResponse>("rest/api/2/issue", new
        {
            Fields = new
            {
                Project = new
                {
                    key = request.ProjectKey
                },
                Summary = request.Summary,
                Description = request.Description,
                Issuetype = new
                {
                    Name = request.Type.ToString()
                }
            }
        });
    }

    public Task<HttpModel<JiraSearchIssueResponse>> SearchIssueAsync(JiraSearchIssueRequest request)
    {
        var queries = new List<string>();
        if (request.IsAssigneeCurrentUser) queries.Add("assignee = currentUser()");
        if (request.Fields is { Count: > 0 } && request.Fields.Exists(field => string.IsNullOrEmpty(field) || !FieldPattern.IsMatch(field)))
            throw new OperationException(FrameworkErrors.JiraRequestIsInvalid);

        var maxResults = request.MaxResults < 1 ? 1 : Math.Min(request.MaxResults, 100);
        var startAt = Math.Max(0, request.StartAt);
        return GetHttpHelper().GetAsync<JiraSearchIssueResponse>("rest/api/2/search", new
        {
            jql = $"{string.Join("AND", queries)} ORDER BY updated DESC",
            startAt,
            maxResults,
            fields = request.Fields.IsNotNullOrEmpty() ? string.Join(',', request.Fields) : "*all"
        });
    }

    public Task<HttpModel<JiraTransitionResponse>> TransitionAsync(JiraTransitionRequest request)
    {
        if (string.IsNullOrEmpty(request.IssueKey) || !IssueKeyPattern.IsMatch(request.IssueKey) ||
            string.IsNullOrEmpty(request.TransitionId) || !TransitionIdPattern.IsMatch(request.TransitionId))
            throw new OperationException(FrameworkErrors.JiraRequestIsInvalid);

        return GetHttpHelper().PostAsync<JiraTransitionResponse>($"rest/api/2/issue/{Uri.EscapeDataString(request.IssueKey)}/transitions", new
        {
            Transition = new { id = request.TransitionId }
        });
    }
}