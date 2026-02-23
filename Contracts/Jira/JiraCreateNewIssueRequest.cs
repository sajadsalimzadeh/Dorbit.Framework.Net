namespace Dorbit.Framework.Contracts.Jira;

public class JiraCreateNewIssueRequest
{
    public string ProjectKey { get; set; }
    public string Summary { get; set; }
    public JiraIssueType Type { get; set; }
}