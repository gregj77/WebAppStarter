using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;

namespace Validation
{
    public abstract class BaselineValidator<T> : AbstractValidator<T>
    {
        protected BaselineValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;
        }

        public override ValidationResult Validate(ValidationContext<T> context)
        {
            return this.StoreResult(context.InstanceToValidate, base.Validate(context));
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = new CancellationToken())
        {
            var result = await base.ValidateAsync(context, cancellation);
            return this.StoreResult(context.InstanceToValidate, result);
        }
    }
}
