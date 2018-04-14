using System;
using System.Collections.Generic;
using Feature01.Models;

namespace Feature01.Data
{
    public interface IDataService
    {
        IObservable<ICollection<Device>> FindDevices(DeviceQueryArguments arguments);
        IObservable<Device> CreateOrUpdateDevice(Device device);
    }
}
