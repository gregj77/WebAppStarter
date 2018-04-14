using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using FluentValidation.Results;
using NLog;

namespace Validation
{
    public class FluentValidationWebApiValidationAttribute : ActionFilterAttribute
    {
        private readonly ValidationPipeline _validationPipeline;

        public FluentValidationWebApiValidationAttribute(ValidationPipeline validationPipeline)
        {
            _validationPipeline = validationPipeline;
        }

        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var logger = (ILogger)actionContext.Request.GetDependencyScope().GetService(typeof(ILogger));

            var method = $"Validation: {actionContext.ActionDescriptor.ControllerDescriptor.ControllerName}.{actionContext.ActionDescriptor.ActionName} ({string.Join(", ", actionContext.ActionArguments.Keys)})";
            logger.Debug(method);

            if (!actionContext.ModelState.IsValid || (await _validationPipeline.ValidateArgumentsAsync(actionContext.ActionArguments, OnValidationError, cancellationToken, actionContext.ModelState)))
            {
                logger.Warn($"{method} - failed with {actionContext.ModelState.Values.SelectMany(p => p.Errors).Count()} error(s)");
                if (logger.IsDebugEnabled)
                {
                    foreach (var model in actionContext.ModelState)
                    {
                        var property = model.Key;
                        foreach (var details in model.Value.Errors)
                        {
                            logger.Debug($"Property: '{property}' -> {details.ErrorMessage ?? details.Exception?.Message}");
                        }
                    }
                }
                actionContext.Response = actionContext.Request.CreateErrorResponse((HttpStatusCode)422, actionContext.ModelState);
                actionContext.Response.ReasonPhrase = "The request failed validation";
            }
        }

        private void OnValidationError(string property, IReadOnlyCollection<ValidationFailure> errors, ModelStateDictionary modelState)
        {
            if (!modelState.ContainsKey(property))
            {
                modelState.Add(property, new ModelState());
            }
            ModelState modelStateErrors = modelState[property];
            foreach (var failure in errors)
            {
                modelStateErrors.Errors.Add(failure.ErrorMessage);
            }
        }
    }
}
