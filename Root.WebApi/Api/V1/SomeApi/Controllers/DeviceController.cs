using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Api.Setup;
using Auth;
using Auth.Models;
using Feature01.EventHandlers.Payload;
using Feature01.Models;
using NLog;
using Quarks.CQRS;
using SomeApi.Models;
using Validation;

namespace SomeApi.Controllers
{
    [RoutePrefix("api")]
    [TokenAuthorize]
    [TokenPermission(TokenClaimType.UserType, nameof(UserType.Standard))]
    public class DeviceController : ApiController
    {
        private readonly IRxQueryDispatcher _queryDispatcher;
        private readonly IAuthUserProvider _userProvider;
        private readonly ILogger _logger;

        public DeviceController(IRxQueryDispatcher queryDispatcher, IAuthUserProvider userProvider, ILogger logger)
        {
            _queryDispatcher = queryDispatcher;
            _userProvider = userProvider;
            _logger = logger;
        }

        [HttpGet]
        [Route("devices", Name = nameof(GetDevices))]
        public Task<IHttpActionResult> GetDevices([FromUri] DeviceQueryArguments arguments)
        {
            return _queryDispatcher
                .Dispatch(new DeviceQuery() { Request = arguments })
                .RemapColletion<Device, DeviceModel>()
                .ToContentResult(this);
        }

        [HttpPost]
        [Route("devices", Name = nameof(RegisterDevice))]
        public Task<IHttpActionResult> RegisterDevice([FromBody] DeviceModel request)
        {
            return request
                .Validate(_logger)
                .Remap<DeviceModel, Device>()
                .Validate(_logger)
                .SelectMany(d => _queryDispatcher.Dispatch(new CreateOrUpdateDeviceQuery { Request = d }))
                .Remap<Device, DeviceModel>()
                .ToCreatedResult(this, nameof(GetDevices), t => t);
        }

        [HttpDelete]
        [Route("devices/{id}")]
        public IHttpActionResult DeleteDevice([FromUri] int id)
        {
            return Ok();
        }

    }
}
