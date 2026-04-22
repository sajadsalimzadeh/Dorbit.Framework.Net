using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Jira;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Utils.Http;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class JiraService(IServiceProvider serviceProvider) : HttpClientApi<ConfigJira>(serviceProvider)
{
    protected override HttpHelper GetHttpHelper()
    {
        var http = GetHttpHelperWithoutClientInfo();
        http.AddHeader("Accept", "application/json");
        http.AddHeader("Accept-Encoding", "none");

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
        return GetHttpHelper().GetAsync<JiraSearchIssueResponse>("rest/api/2/search", new
        {
            jql = $"{string.Join("AND", queries)} ORDER BY updated DESC",
            startAt = request.StartAt,
            maxResults = request.MaxResults,
            fields = request.Fields.IsNotNullOrEmpty() ? string.Join(',', request.Fields) : "*all"
        });
    }

    public Task<HttpModel<JiraTransitionResponse>> TransitionAsync(JiraTransitionRequest request)
    {
        return GetHttpHelper().PostAsync<JiraTransitionResponse>($"rest/api/2/issue/{request.IssueKey}/transitions", new
        {
            Transition = new { id = request.TransitionId }
        });
    }
}