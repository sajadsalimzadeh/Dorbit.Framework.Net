namespace Dorbit.Framework.Contracts.Identities;

public class IdentityRequest
{
    public string AccessToken { get; set; }
    public string CsrfToken { get; set; }
    public string UserAgent { get; set; }
    public string Timezone { get; set; }

    public string IpV4 { get; set; }
    public string IpV6 { get; set; }
    public LatLng Location { get; set; }
    
    public IdentityRequest()
    {
    }

    public IdentityRequest(IdentityRequest baseRequest)
    {
        AccessToken = baseRequest.AccessToken;
        CsrfToken = baseRequest.CsrfToken;
        UserAgent = baseRequest.UserAgent;
        Timezone = baseRequest.Timezone;
        IpV4 = baseRequest.IpV4;
        IpV6 = baseRequest.IpV6;
        Location = baseRequest.Location;
    }
}