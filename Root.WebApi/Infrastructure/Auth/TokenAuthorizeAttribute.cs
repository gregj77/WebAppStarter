using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Threading.Tasks;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Auth
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class TokenAuthorizeAttribute : AuthorizeAttribute
    {
        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var accessHelper = new AccessFlowHelper<HttpActionContext>(
                ResolveAuthServiceFromContext(actionContext),
                GetToken(actionContext),
                UserAuthenticated,
                HandleUnauthorizedRequest,
                GetAccessPermissions);
            return accessHelper.AuthenticateAndAuthorizeAsync(actionContext).ToTask(cancellationToken);
        }

        private IAuthService ResolveAuthServiceFromContext(HttpActionContext ctx)
        {
            return (IAuthService)ctx.Request.GetDependencyScope().GetService(typeof(IAuthService));
        }

        private string GetToken(HttpActionContext ctx)
        {
            return ctx?.Request?.Headers?.Authorization?.Parameter;
        }

        private void UserAuthenticated(HttpActionContext ctx, ClaimsPrincipal principal)
        {
            ctx.RequestContext.Principal = principal;
        }

        private IEnumerable<TokenPermissionAttribute> GetAccessPermissions(HttpActionContext ctx)
        {
            return ctx
                .ControllerContext
                .Controller
                .GetType()
                .GetCustomAttributes(typeof(TokenPermissionAttribute), true)
                .Cast<TokenPermissionAttribute>()
                .Concat(ctx
                    .ActionDescriptor
                    .GetCustomAttributes<TokenPermissionAttribute>(true))
                .Distinct();
        }
    }
}
