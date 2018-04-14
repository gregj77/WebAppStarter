using System;
using System.Collections.Generic;
using Feature01.Models;

namespace Feature01.Services
{
    public interface IDeviceService
    {
        IObservable<ICollection<Device>> FindDevices(DeviceQueryArguments arguments);
        IObservable<Device> AddOrUpdateDevice(Device device);
    }
}
