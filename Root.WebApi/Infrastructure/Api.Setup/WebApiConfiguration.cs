using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using System.Web.Http.Validation;
using Autofac;
using Autofac.Integration.WebApi;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Validation;

namespace Api.Setup
{
    internal static class WebApiConfiguration
    {
        public static HttpConfiguration ConfigureWebApi(this HttpConfiguration httpConfiguration, IContainer container)
        {
            httpConfiguration.Services.Replace(typeof(IHttpControllerTypeResolver), new WebApiControllerTypeResolver());
            httpConfiguration.MapHttpAttributeRoutes(new RouteProvider());
            httpConfiguration.Routes.Add("NotFoundApi", new HttpRoute("api/{*url}", new HttpRouteValueDictionary(new 
            {
                controller = "Error",
                action = "NotFound"
            })));
            httpConfiguration.EnableCors(new EnableCorsAttribute("*", "*", "*"));
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            httpConfiguration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            var factory = container.Resolve<IValidatorFactory>();
            var validationPipeline = new ValidationPipeline(factory);
            var validationFilter = new FluentValidationWebApiValidationAttribute(validationPipeline);
            httpConfiguration.Filters.Add(validationFilter);
            var modelValidationServices = httpConfiguration.Services.GetServices(typeof(ModelValidatorProvider));
            foreach (var modelValidationService in modelValidationServices)
            {
                httpConfiguration.Services.Remove(typeof(ModelValidatorProvider), modelValidationService);
            }
            return httpConfiguration;
        }

        private class RouteProvider : DefaultDirectRouteProvider
        {
            public override IReadOnlyList<RouteEntry> GetDirectRoutes(HttpControllerDescriptor controllerDescriptor, IReadOnlyList<HttpActionDescriptor> actionDescriptors,
                IInlineConstraintResolver constraintResolver)
            {
                var result = base.GetDirectRoutes(controllerDescriptor, actionDescriptors, constraintResolver);
                return result;
            }
        }

        public class WebApiControllerTypeResolver : DefaultHttpControllerTypeResolver
        {
            public WebApiControllerTypeResolver() : base(t => t.Name.EndsWith("Controller"))
            {
            }

            public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
            {
                var found = base.GetControllerTypes(assembliesResolver);
                return found;
            }
        }
    }
}
