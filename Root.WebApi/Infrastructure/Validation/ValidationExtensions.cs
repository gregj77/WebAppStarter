using System;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using NLog;

namespace Validation
{
    public static class ValidationExtensions
    {
        internal static IValidatorFactory ValidatorFactory;

        private static readonly ConditionalWeakTable<object, ValidationResult> ValidationCache = new ConditionalWeakTable<object, ValidationResult>();

        public static ValidationResult EnsureModelIsValid<TModel>(this TModel model)
        {
            if (null == model)
                throw new ArgumentNullException(nameof(model));

            if (ValidationCache.TryGetValue(model, out var result))
            {
                return result;
            }
            return InternalValidate(model);
        }

        public static async Task<ValidationResult> EnsureModelIsValidAsync<TModel>(this TModel model)
        {
            if (null == model)
                throw new ArgumentNullException(nameof(model));

            if (ValidationCache.TryGetValue(model, out var result))
            {
                return result;
            }
            return await InternalValidateAsync(model);
        }

        public static IObservable<TModel> Validate<TModel>(this IObservable<TModel> stream, ILogger logger = null)
        {
            return stream.SelectMany(model => Validate(model, logger));
        }

        public static IObservable<TModel> Validate<TModel>(this TModel model, ILogger logger = null)
        {
            return Observable.Create<TModel>(async o =>
            {
                ValidationResult result;
                if (null != model)
                {
                    result = await model.EnsureModelIsValidAsync();
                }
                else
                {
                    result = new ValidationResult(new[] { new ValidationFailure(string.Empty, "Model is not set to a valid reference") });
                }

                if (!result.IsValid)
                {
                    var error = new ValidationException($"The model {model?.GetType().FullName ?? "<null>"} is invalid", result.Errors);
                    o.OnError(error);
                }
                else
                {
                    o.OnNext(model);
                    o.OnCompleted();
                }
            });
        }

        public static void ThrowIfModelIsInvalid<TModel>(this TModel model, ILogger logger)
        {
            var result = EnsureModelIsValid(model);
            if (!result.IsValid)
            {
                logger?.Warn("Trying to execute business logic with invalid model - erorr(s): " +
                                $"{result.Errors.Count}, " +
                                $"{result.Errors.Aggregate(new StringBuilder(), (bldr, failure) => bldr.Append(failure.ErrorMessage).Append(", "))}");

                throw new ValidationException($"The model {model.GetType().FullName} is invalid", result.Errors);
            }
        }

        public static async Task ThrowIfModelIsInvalidAsync<TModel>(this TModel model, ILogger logger = null)
        {
            var result = await EnsureModelIsValidAsync(model);
            if (!result.IsValid)
            {
                logger?.Warn("Trying to execute business logic with invalid model - erorr(s): " +
                                $"{result.Errors.Count}, " +
                                $"{result.Errors.Aggregate(new StringBuilder(), (bldr, failure) => bldr.Append(failure.ErrorMessage).Append(", "))}");

                throw new ValidationException($"The model {model.GetType().FullName} is invalid", result.Errors);
            }
        }

        internal static ValidationResult StoreResult<TModel>(this IValidator<TModel> validator, TModel model, ValidationResult result)
        {
            if (!ValidationCache.TryGetValue(model, out var tmp))
            {
                ValidationCache.Add(model, result);
            }
            return result;
        }

        private static async Task<ValidationResult> InternalValidateAsync(object model)
        {
            var validator = ValidatorFactory.GetValidator(model.GetType());
            if (null == validator)
            {
                throw new InvalidOperationException("Validator for model " + model.GetType().FullName + " not found!");
            }
            var result = await validator.ValidateAsync(model);

            if (!ValidationCache.TryGetValue(model, out var tmp))
            {
                ValidationCache.Add(model, result);
            }
            return result;
        }

        private static ValidationResult InternalValidate(object model)
        {
            var validator = ValidatorFactory.GetValidator(model.GetType());
            if (null == validator)
            {
                throw new InvalidOperationException("Validator for model " + model.GetType().FullName + " not found!");
            }
            var result = validator.Validate(model);
            if (!ValidationCache.TryGetValue(model, out var tmp))
            {
                ValidationCache.Add(model, result);
            }
            return result;
        }
    }
}
