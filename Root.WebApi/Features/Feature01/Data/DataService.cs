using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using Feature01.Models;

namespace Feature01.Data
{
    internal class DataService : IDataService
    {
        private readonly SynchronizationContext _ctx;
        private static readonly IDictionary<long, Device> Devices = new ConcurrentDictionary<long, Device>();

        static DataService()
        {
            Devices.Add(1, new Device { Id = 1, RegionId = 1, DeviceName = "foo1", Description = "bar1"});
            Devices.Add(2, new Device { Id = 2, RegionId = 2, DeviceName = "foo2", Description = "bar2" });
        }

        public DataService(SynchronizationContext ctx)
        {
            _ctx = ctx;
        }

        public IObservable<ICollection<Device>> FindDevices(DeviceQueryArguments arguments)
        {
            return Observable.Start(() => Devices.Values).ObserveOn(_ctx);
        }

        public IObservable<Device> CreateOrUpdateDevice(Device device)
        {
            return Observable.Start(() =>
                {
                    if (device.Id > 0)
                    {
                        Devices[device.Id] = device;
                    }
                    else
                    {
                        int id = Devices.Count + 1;
                        device.Id = id;
                        Devices[id] = device;

                    }
                    Thread.Sleep(1000);
                    return device;
                })
                .ObserveOn(_ctx);
        }
    }
}
