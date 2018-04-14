using System;
using Autofac;
using FluentValidation;
using NLog;
using IValidator = FluentValidation.IValidator;

namespace Validation

{
    internal class AutofacValidatorFactory : ValidatorFactoryBase
    {
        private readonly IComponentContext _context;
        private readonly ILogger _logger;

        public AutofacValidatorFactory(IComponentContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public override IValidator CreateInstance(Type validatorType)
        {
            if (_context.IsRegistered(validatorType))
            {
                try
                {
                    var result = (IValidator) _context.Resolve(validatorType);

                    if (_logger.IsTraceEnabled)
                        _logger.Trace("Found validator for model: {0} -> validatorType: {1}", validatorType.GetGenericArguments()[0].FullName, result.GetType().FullName);

                    return result;
                }
                catch (Exception err)
                {
                    _logger.Error($"Failed to resolve validator {validatorType.FullName} due to <{err.GetType().FullName}>{err.Message}\n{err.StackTrace}");
                }
            }

            if (_logger.IsTraceEnabled)
                _logger.Trace("No validator found for type {0}. Assuming model is valid.", validatorType.GetGenericArguments()[0].FullName);

            return null;
        }
    }
}
