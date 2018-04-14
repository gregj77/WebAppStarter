using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Http.Controllers;
using Auth.Models;

namespace Auth
{
    public static class HttpRequestContextExtensions
    {
        public static long GetUserId(this HttpRequestContext context)
        {
            var claim = GetClaim(context, TokenClaimType.UserId);
            return long.Parse(claim.Value);
        }

        public static string GetUserName(this HttpRequestContext context)
        {
            var claim = GetClaim(context, TokenClaimType.UserName);
            return claim.Value;
        }

        
        private static Claim GetClaim(HttpRequestContext context, string claimType)
        {

            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
            if (null == claimsIdentity || !claimsIdentity.IsAuthenticated)
                throw new UnauthorizedAccessException("No ClaimsIdentity is present");

            var claim = claimsIdentity.Claims.FirstOrDefault(c => c.Type == claimType);
            if (null == claim)
                throw new UnauthorizedAccessException($"No {claimType} claim is present");

            return claim;
        }
    }
}
