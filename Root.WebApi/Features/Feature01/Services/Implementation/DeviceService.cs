using System;
using System.Collections.Generic;
using Feature01.Data;
using Feature01.Models;
using NLog;
using Utils;

namespace Feature01.Services.Implementation
{
    internal class DeviceService : IDeviceService
    {
        private readonly IDataService _dataService;
        private readonly ILogger _logger;

        public DeviceService(IDataService dataService, ILogger logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        public IObservable<ICollection<Device>> FindDevices(DeviceQueryArguments arguments)
        {
            arguments = arguments ?? new DeviceQueryArguments();
            return _dataService
                .FindDevices(arguments)
                .LogInfo(_logger, devices => $"found {devices.Count} devices for query {arguments}")
                .LogException(_logger);
        }

        public IObservable<Device> AddOrUpdateDevice(Device device)
        {
            return _dataService
                .CreateOrUpdateDevice(device)
                .LogInfo(_logger, d => $"device {d.Id} stored in repository")
                .LogException(_logger);
        }

    }
}
