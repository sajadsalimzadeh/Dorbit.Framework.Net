namespace Dorbit.Framework.Contracts.Jira;

public class JiraIssueFields
{
    public string Summary { get; set; }
    public string Description { get; set; }
    public JiraProject Project { get; set; }

    // public T GetCustomFieldValue<T>(int id)
    // {
    //     if (TryGetValue($"customfield_{id}", out var value))
    //     {
    //         return (T)Convert.ChangeType(value, typeof(T));
    //     }
    //
    //     return default;
    // }
}