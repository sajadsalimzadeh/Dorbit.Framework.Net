namespace Dorbit.Framework.Contracts;

public class IdentityValidateRequest
{
    public string AccessToken { get; set; }
    public string CsrfToken { get; set; }
    public string UserAgent { get; set; }
    public string Access { get; set; }
}