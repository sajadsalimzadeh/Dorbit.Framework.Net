using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.Jira;

public class JiraSearchIssueRequest
{
    public bool IsAssigneeCurrentUser { get; set; }
    public int StartAt { get; set; } = 0;
    public int MaxResults { get; set; } = 100;
    public List<string> Fields { get; set; }
}