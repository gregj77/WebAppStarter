using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using Api.Setup.DependencyInjection;
using Autofac;
using NLog;
using NLog.Config;

namespace Api.Setup
{
    public abstract class HttpApplicationBase : HttpApplication
    {
        private const string LanguageQueryParam = "language";
        private static ILogger _logger;

        protected void Application_Start()
        {
            ConfigurationItemFactory
                .Default
                .LayoutRenderers
                .RegisterDefinition("call-context", typeof(UserContextRenderer));

            var loadedAssemblies = AppDomain
                .CurrentDomain.GetAssemblies()
                .Where(p => !p.IsDynamic)
                .Where(p => CanConfigureAssembly(p.GetName().Name))
                .Concat(OnConfigureModules())
                .ToArray();
            
            var container = ContainerConfig.ConfigureDependencies(loadedAssemblies);
            _logger = container.Resolve<ILogger>();
            _logger.Info("Application started {0:x8}", GetHashCode());
        }

        protected void Application_BeginRequest(object sender, EventArgs args)
        {
            var languageArg = HttpContext.Current.Request.Params[LanguageQueryParam]
                              ?? HttpContext.Current.Request.UserLanguages?.FirstOrDefault();

            if (string.IsNullOrEmpty(languageArg)) return;

            try
            {
                var culture = CultureInfo.GetCultureInfo(languageArg);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        protected void Application_EndRequest(object sender, EventArgs args)
        {
            // no erorr, no action
            if (Response.StatusCode < 300) return;
            // WEB API methods are not handled by mvc pipeline
            if (Request.RawUrl.StartsWith("/api/", StringComparison.OrdinalIgnoreCase)) return;
            // already executing error handler
            if (Request.Path.StartsWith("/error/", StringComparison.OrdinalIgnoreCase)) return;

            // make sure proper error page is displayed
            if (Response.StatusCode == 404) Server.TransferRequest("/Error/NotFound");
            if (Response.StatusCode >= 500) Server.TransferRequest("/Error/Error");
        }

        protected void Application_Error(object sender, EventArgs args)
        {
            var error = Server.GetLastError();
            _logger.Error("Application error {0}: {1}\n{2}", error.GetType().FullName, error.Message, error.StackTrace ?? string.Empty);

            var unauthorizedAccess = error as UnauthorizedAccessException;
            var inner = error.InnerException;
            while (inner != null)
            {
                _logger.Error("\tInner error: {0}: {1}\n{2}", inner.GetType().FullName, inner.Message, inner.StackTrace);
                inner = inner.InnerException;
                unauthorizedAccess = unauthorizedAccess ?? (inner as UnauthorizedAccessException);
            }

            if (null != unauthorizedAccess)
            {
                Server.ClearError();
                Server.TransferRequest("/Error/NotAuthorized?reason=" + HttpUtility.UrlEncode(unauthorizedAccess.Message));
            }
        }

        protected abstract Assembly[] OnConfigureModules();

        protected abstract bool CanConfigureAssembly(string assemblyName);
    }
}
