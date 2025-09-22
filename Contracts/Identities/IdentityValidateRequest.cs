namespace Dorbit.Framework.Contracts.Identities;

public class IdentityValidateRequest
{
    public string AccessToken { get; set; }
    public string CsrfToken { get; set; }
    public string UserAgent { get; set; }
    
    public string IpV4 { get; set; }
    public string IpV6 { get; set; }
    public string Timezone { get; set; }
    public string PointerToken { get; set; }
}