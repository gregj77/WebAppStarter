using System.Collections.Generic;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Feature01.EventHandlers.Payload;
using Feature01.Models;
using Feature01.Services;
using NLog;
using Quarks.CQRS;
using Utils;

namespace Feature01.EventHandlers
{
    public class CreateOrUpdateDeviceQueryHandler : IQueryHandler<CreateOrUpdateDeviceQuery, Device>
    {
        private readonly IDeviceService _deviceService;
        private readonly ILogger _logger;

        public CreateOrUpdateDeviceQueryHandler(IDeviceService deviceService, ILogger logger)
        {
            _deviceService = deviceService;
            _logger = logger;
        }

        public Task<Device> HandleAsync(CreateOrUpdateDeviceQuery query, CancellationToken cancellationToken)
        {
            return _deviceService
                .AddOrUpdateDevice(query.Request)
                .LogInfo(_logger, result => $"created/updated device {result.Id}")
                .LogException(_logger)
                .ToTask(cancellationToken);
        }
    }
}
