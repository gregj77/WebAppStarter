using System.Security.Claims;
using System.Text;
using System.Web;
using Auth;
using Auth.Models;
using NLog;
using NLog.LayoutRenderers;

namespace Api.Setup
{
    [LayoutRenderer("call-context")]
    public class UserContextRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            HttpContext context = null;
            try
            {
                context = HttpContext.Current;
                if (null != context)
                {
                    var url = $"{context.Request.RequestType} {context.Request.Url.LocalPath}";
                    var user = context.User as ClaimsPrincipal;

                    builder.Append("0x");
                    builder.Append(context.GetHashCode().ToString("X8"));
                    builder.Append("|");
                    builder.Append(url);
                    builder.Append("|");
                    builder.Append(null == user || !user.Identity.IsAuthenticated ? "anonymous" : user.GetClaim<string>(TokenClaimType.UserName));
                    builder.Append("[");
                    builder.Append(null == user || !user.Identity.IsAuthenticated ? "(null)" : user.GetClaim<string>(TokenClaimType.UserId));
                    builder.Append("]");
                    return;
                }
            }
            catch
            {
            }
            builder.Append("N/A");
        }
    }
}