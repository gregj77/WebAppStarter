using System.Security.Claims;
using System.Threading;

namespace Auth
{
    public class AuthUserProvider : IAuthUserProvider
    {
        public ClaimsPrincipal CurrentUser => Thread.CurrentPrincipal as ClaimsPrincipal;

        public TClaim GetUserClaim<TClaim>(string claimType)
        {
            return CurrentUser.GetClaim<TClaim>(claimType);
        }
    }
}