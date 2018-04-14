namespace Auth.Models
{
    public class TokenClaim
    {
        public long UserId { get; set; }
        public string ClaimName { get; set; }
        public string ClaimValue { get; set; }
    }
}
