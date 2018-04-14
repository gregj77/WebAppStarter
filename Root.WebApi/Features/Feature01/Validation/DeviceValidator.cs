using Feature01.Models;
using FluentValidation;
using Validation;

namespace Feature01.Validation
{
    internal class DeviceValidator : BaselineValidator<Device>
    {
        public DeviceValidator()
        {
            RuleFor(x => x.DeviceName)
                .NotEmpty()
                .MaximumLength(300);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(300);
        }
    }
}
