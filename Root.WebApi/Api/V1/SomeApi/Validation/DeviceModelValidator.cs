using FluentValidation;
using SomeApi.Models;
using Validation;

namespace SomeApi.Validation
{
    public class DeviceModelValidator : BaselineValidator<DeviceModel>
    {
        public DeviceModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .MinimumLength(10);

            RuleFor(x => x.DeviceId)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(100)
                .MinimumLength(10);
        }
    }
}