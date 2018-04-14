using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;

namespace Auth
{
    public static class ClaimsPrincipalExtensions
    {
        public static TClaim GetClaim<TClaim>(this IPrincipal principal, string claimType, Func<TClaim, bool> filter = null)
        {
            if (null == principal) throw new ArgumentNullException();
            var claimsPrincipal = (ClaimsPrincipal)principal;
            return claimsPrincipal
                .FindAll(p => string.Equals(p.Type, claimType, StringComparison.OrdinalIgnoreCase))
                .Select(p =>
                {
                    var parseMethod = typeof (TClaim).GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new[] {typeof (string)}, null);
                    if (null != parseMethod)
                    {
                        return (TClaim) parseMethod.Invoke(null, new object[] { p.Value });
                    }
                    return (TClaim) Convert.ChangeType(p.Value, typeof (TClaim));
                })
                .First(v => filter?.Invoke(v) ?? true);
        }
    }
}
