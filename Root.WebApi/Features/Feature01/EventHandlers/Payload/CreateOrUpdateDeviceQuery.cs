using System.Collections.Generic;
using Feature01.Models;
using Quarks.CQRS;

namespace Feature01.EventHandlers.Payload
{
    public class CreateOrUpdateDeviceQuery : IQuery<Device>
    {
        public Device Request { get; set; }
    }
}
