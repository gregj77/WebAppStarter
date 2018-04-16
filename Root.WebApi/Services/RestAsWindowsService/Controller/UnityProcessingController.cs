using NLog;
using System.Threading.Tasks;
using System.Web.Http;

namespace RestAsWindowsService.Controller
{
    [RoutePrefix("api/internal/unity")]
    public class UnityProcessingController : ApiController 
    {
        private readonly ILogger _logger;

        public UnityProcessingController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("")]
        public Task<IHttpActionResult> CheckStatus()
        {
            // things to do:
            // 1. setup service / command to handle any logic 
            // 2. delegate call to the service
            // 3. service to implement queueing
            return Task.FromResult((IHttpActionResult)Ok());
        }
    }
}
