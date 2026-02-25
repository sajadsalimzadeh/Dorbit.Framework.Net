namespace Dorbit.Framework.Contracts.Jira;

public class JiraTransitionRequest
{
    public string IssueKey { get; set; }
    public string TransitionId { get; set; }
}