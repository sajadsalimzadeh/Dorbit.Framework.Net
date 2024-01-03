namespace Dorbit.Framework.Models.Jwts
{
    public class JwtCreateTokenRequest
    {
        public DateTime? Expires { get; set; }
        public Dictionary<string, string> Claims { get; set; }
    }
}