using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Auth;
using Auth.Models;
using ePatrol.Api.Setup;
using FluentValidation;
using NLog;
using Utils;

namespace Api.Setup.Controllers
{
    [RoutePrefix("api")]
    public class AuthorizationController : ApiController
    {
        private readonly IAuthService _authorizationService;
        private readonly ILogger _logger;

        public AuthorizationController(IAuthService authorizationService, ILogger logger)
        {
            _authorizationService = authorizationService;
            _logger = logger;
        }

        [HttpPost]
        [Route("token")]
        public Task<IHttpActionResult> GetToken(TokenRequest request)
        {
            return _authorizationService
                .Authorize(request, HttpContext.Current.GetOwinContext())
                .Catch<Token, NotSupportedException>(err => Observable.Throw<Token>(new ValidationException(err.Message)))
                .LogInfo(_logger, tkn => $"user {request.Username} authorized")
                .LogException(_logger, err => $"failed to authorize user {request.Username} - {err.Message}<{err.GetType().FullName}>")
                .ToContentResult(this);
        }    
    }
}
