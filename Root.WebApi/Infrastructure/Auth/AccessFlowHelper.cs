using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace Auth
{
    internal class AccessFlowHelper<TContext>
    {
        private readonly IAuthService _authService;
        private readonly string _token;
        private readonly Action<TContext, ClaimsPrincipal> _successHandler;
        private readonly Action<TContext> _unauthorizedHandler;
        private readonly Func<TContext, IEnumerable<TokenPermissionAttribute>> _permissionsResolver;

        public AccessFlowHelper(
            IAuthService authService,
            string token,
            Action<TContext, ClaimsPrincipal> successHandler, 
            Action<TContext> unauthorizedHandler,
            Func<TContext, IEnumerable<TokenPermissionAttribute>> permissionsResolver)
        {
            _authService = authService;
            _token = token;
            _successHandler = successHandler;
            _unauthorizedHandler = unauthorizedHandler;
            _permissionsResolver = permissionsResolver;
        }

        public IObservable<Unit> AuthenticateAndAuthorizeAsync(TContext context)
        {
            if (string.IsNullOrEmpty(_token))
            {
                _unauthorizedHandler(context);
                return Observable.Return(Unit.Default);
            }

            return Observable
                .Zip(
                    _authService.GetSessionUserId(_token),
                    _authService.GetIdentityFromToken(_token).Select(identity => new ClaimsPrincipal(identity)), 
                    Observable.Return(context),
                    Tuple.Create)
                .Select(TryAuthorize)
                .Catch<Unit, Exception>(err =>
                {
                    _unauthorizedHandler(context);
                    return Observable.Return(Unit.Default);
                });
        }

        public void AuthenticateAndAuthorize(TContext context)
        {
            Exception error = null;
            using (var mre = new ManualResetEventSlim(false))
            {
                ThreadPool.QueueUserWorkItem(state => AuthenticateAndAuthorizeAsync(context).Subscribe(_ => { }, ((ManualResetEventSlim)state).Set), mre);
                mre.Wait();
            }
            if (null != error)
            {
                throw error;
            }
        }

        private Unit TryAuthorize(Tuple<long, ClaimsPrincipal, TContext> result)
        {
            Thread.CurrentPrincipal = result.Item2;
            _successHandler(result.Item3, result.Item2);
            Authorize(_permissionsResolver(result.Item3));
            return Unit.Default;
        }

        private void Authorize(IEnumerable<TokenPermissionAttribute> requiredPermissions)
        {
            var claims = ClaimsPrincipal.Current?.Claims ?? Enumerable.Empty<Claim>();

            var result = requiredPermissions
                .Where(required => !claims.Any(required.Equals))
                .Aggregate((StringBuilder) null, (bldr, current) =>
                {
                    bool addComma = bldr != null;
                    bldr = bldr ?? new StringBuilder("Access denied - missing required claim(s): ");
                    if (addComma) bldr.Append(", ");
                    bldr.Append(current.ClaimType);
                    bldr.Append("='");
                    bldr.Append(current.Value);
                    bldr.Append("'");
                    return bldr;
                });

            if (null != result)
            {
                throw new UnauthorizedAccessException(result.ToString());
            }
        }
    }
}
