using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;

namespace Validation
{
    public class ValidationPipeline
    {
        public delegate void OnValidationErrorDelegate<in TCustomState>(string propertyName, IReadOnlyCollection<ValidationFailure> errors, TCustomState state);

        private readonly IValidatorFactory _factory;

        public ValidationPipeline(IValidatorFactory factory)
        {
            _factory = factory;
        }

        public bool ValidateArguments<TCustomState>(IDictionary<string, object> arguments, OnValidationErrorDelegate<TCustomState> onError, TCustomState state)
        {
            return ValidateInternal(arguments, onError, CancellationToken.None, false, state).Result;
        }

        public Task<bool> ValidateArgumentsAsync<TCustomState>(IDictionary<string, object> arguments, OnValidationErrorDelegate<TCustomState> onError, CancellationToken cancellationToken, TCustomState state)
        {
            return ValidateInternal(arguments, onError, cancellationToken, true, state);
        }

        private async Task<bool> ValidateInternal<TCustomState>(IDictionary<string, object> arguments, OnValidationErrorDelegate<TCustomState> onError, CancellationToken cancellationToken, bool runAsync, TCustomState state)
        {
            if (null == arguments) throw new ArgumentNullException(nameof(arguments));
            if (null == onError) throw new ArgumentNullException(nameof(onError));

            bool hasErrors = false;
            foreach (var arg in arguments.Where(arg => arg.Value != null && !(arg.Value.GetType().IsPrimitive || arg.Value is string)))
            {
                var validator = _factory.GetValidator(arg.Value.GetType());
                if (null == validator) continue;

                ValidationResult result;
                try
                {
                    if (runAsync)
                        result = await validator.ValidateAsync(arg.Value, cancellationToken);
                    else
                        result = validator.Validate(arg.Value);

                    if (arg.Value != null && arg.Value.GetType().IsArray)
                    {
                        Array array = (Array)arg.Value;
                        for (int i = 0; i < array.Length; ++i)
                        {
                            var obj = array.GetValue(i);
                            if (null != obj)
                            {
                                var itemValidator = _factory.GetValidator(obj.GetType());
                                var propertyChain = new PropertyChain(new[] { "Item" });
                                propertyChain.AddIndexer(i);
                                var ctx = new ValidationContext(
                                    obj, 
                                    propertyChain,
                                    ValidatorOptions.ValidatorSelectors.DefaultValidatorSelectorFactory());

                                ValidationResult localResult;
                                if (runAsync)
                                    localResult = await itemValidator.ValidateAsync(ctx, cancellationToken);
                                else
                                    localResult = itemValidator.Validate(ctx);

                                if (result == null)
                                {
                                    result = localResult;
                                }
                                if (!localResult.IsValid)
                                {
                                    foreach (var failure in localResult.Errors)
                                    {
                                        result.Errors.Add(failure);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (ValidationException err)
                {
                    var failures = err.Errors.Select(p => new ValidationFailure(p.PropertyName, p.ErrorMessage));
                    result = new ValidationResult(failures);
                }
                catch (Exception err)
                {
                    var failures = new[] { new ValidationFailure(arg.Key, err.Message) };
                    result = new ValidationResult(failures);
                }

                if (result.IsValid) continue;

                hasErrors = true;

                foreach (var group in result.Errors.GroupBy(p => p.PropertyName, StringComparer.OrdinalIgnoreCase))
                {
                    onError($"{arg.Key}.{group.Key}", group.ToArray(), state);
                }
            }
            return hasErrors;
        }    
    }
}
