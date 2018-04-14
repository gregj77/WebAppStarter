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
    public class DeviceQueryHandler : IQueryHandler<DeviceQuery, ICollection<Device>>
    {
        private readonly IDeviceService _deviceService;
        private readonly ILogger _logger;

        public DeviceQueryHandler(IDeviceService deviceService, ILogger logger)
        {
            _deviceService = deviceService;
            _logger = logger;
        }

        public Task<ICollection<Device>> HandleAsync(DeviceQuery query, CancellationToken cancellationToken)
        {
            return _deviceService
                .FindDevices(query.Request)
                .LogInfo(_logger, result => $"got {result.Count} devices")
                .LogException(_logger)
                .ToTask(cancellationToken);
        }
    }
}
