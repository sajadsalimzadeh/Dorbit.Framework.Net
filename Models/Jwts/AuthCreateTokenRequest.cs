namespace Dorbit.Framework.Models.Jwts
{
    public class AuthCreateTokenRequest
    {
        public DateTime? Expires { get; set; }
        public Dictionary<string, string> Claims { get; set; }
    }
}