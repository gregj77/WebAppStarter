using Utils.Data;

namespace Feature01.Models
{
    public class DeviceQueryArguments : QueryArguments
    {
        public DeviceQueryArguments()
        {
            OrderBy = "DeviceName";
        }   
    }
}