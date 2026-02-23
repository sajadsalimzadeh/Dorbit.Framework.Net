using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Jira;
using Dorbit.Framework.Contracts.Results;
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

        var authenticationString = $"{Config.Username}:{Config.ApiKey.GetDecryptedValue()}";
        var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
        http.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

        return http;
    }

    public Task<HttpModel<JiraCreateNewIssueResponse>> CreateNewIssueAsync(JiraCreateNewIssueRequest request)
    {
        return GetHttpHelper().PostAsync<JiraCreateNewIssueResponse>("rest/api/3/issue", new
        {
            Fields = new
            {
                Project = new
                {
                    key = request.ProjectKey
                },
                Summary = request.Summary,
                Issuetype = new
                {
                    Name = request.Type.ToString()
                }
            }
        });
    }
}