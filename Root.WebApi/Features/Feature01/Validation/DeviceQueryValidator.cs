using System.Collections.Generic;
using Feature01.Models;
using Utils;
using Validation;

namespace Feature01.Validation
{
    public class DeviceQueryValidator : QueryArgumentsValidator<DeviceQueryArguments>
    {
        public DeviceQueryValidator(FilterHelper helper) : base(helper)
        {
        }

        protected override IEnumerable<FilterHelper.FilterItem> FilterProperties
        {
            get
            {
                yield return new FilterHelper.FilterItem(typeof(Device).GetProperty(nameof(Device.DeviceName)));
                yield return new FilterHelper.FilterItem(typeof(Device).GetProperty(nameof(Device.Description)));
                yield return new FilterHelper.FilterItem(typeof(Device).GetProperty(nameof(Device.Id)));
            }
        }

        protected override IEnumerable<string> SortProperties
        {
            get { yield return nameof(Device.DeviceName); }
        }
    }
}
