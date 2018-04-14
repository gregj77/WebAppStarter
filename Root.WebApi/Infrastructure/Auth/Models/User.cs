using System.Collections.Generic;
using System.Security.Claims;

namespace Auth.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordHash { get; set; }
        public string Password { get; set; }


        internal IEnumerable<Claim> GetClaims()
        {
            yield return new Claim(TokenClaimType.Name, Name);
            yield return new Claim(TokenClaimType.Surname, Surname);
            yield return new Claim(TokenClaimType.UserName, Username);
            yield return new Claim(TokenClaimType.UserId, Id.ToString());
            yield return new Claim(TokenClaimType.Email, Email);
            yield return new Claim(TokenClaimType.UserType, UserType.Standard.ToString());
        }
    }
}
