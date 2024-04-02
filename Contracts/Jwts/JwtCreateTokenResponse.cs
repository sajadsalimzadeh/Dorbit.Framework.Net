namespace Dorbit.Framework.Contracts.Jwts
{
    public class JwtCreateTokenResponse
    {
        public string Key { get; set; }
        public string Csrf { get; set; }
    }
}