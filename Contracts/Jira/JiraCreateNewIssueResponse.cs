using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.Jira;

public class JiraCreateNewIssueResponse
{
    public string Id { get; set; }
    public string Key { get; set; }
    public string Self { get; set; }
    public List<string> ErrorMessages { get; set; }
    public Dictionary<string, object> Errors { get; set; }
}