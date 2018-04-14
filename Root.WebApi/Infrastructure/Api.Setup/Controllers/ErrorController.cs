using System.Net;
using System.Net.Http;
using System.Web.Http;
using NLog;

namespace Api.Setup.Controllers
{
    public class ErrorController : ApiController
    {
        private readonly ILogger _logger;

        public ErrorController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public new void NotFound()
        {
            string msg = $"The resource '{Request.RequestUri}' doesn't exist";
            _logger.Warn($"Returning 404 (NotFound) - resource '{Request.RequestUri}' not found!");
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(msg)
            };
            throw new HttpResponseException(responseMessage);
        }
    }
}