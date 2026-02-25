namespace Dorbit.Framework.Contracts.Jira;

public class JiraIssue
{
    public string Expand { get; set; }
    public string Id { get; set; }
    public string Self { get; set; }
    public string Key { get; set; }
    public JiraIssueFields Fields { get; set; }
}