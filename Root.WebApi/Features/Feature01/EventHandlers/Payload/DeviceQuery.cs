using System.Collections.Generic;
using Feature01.Models;
using Quarks.CQRS;

namespace Feature01.EventHandlers.Payload
{
    public class DeviceQuery : IQuery<ICollection<Device>>
    {
        public DeviceQueryArguments Request { get; set; }
    }
}
