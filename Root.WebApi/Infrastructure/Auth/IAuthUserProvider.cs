using System.Security.Claims;

namespace Auth
{
    public interface IAuthUserProvider
    {
        ClaimsPrincipal CurrentUser { get; }
        TClaim GetUserClaim<TClaim>(string claimType);
    }
}
