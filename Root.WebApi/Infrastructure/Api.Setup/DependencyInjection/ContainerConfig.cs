using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;
using Api.Setup.Modules;
using Auth;
using Autofac;
using Autofac.Integration.WebApi;
using Utils;
using Validation;

namespace Api.Setup.DependencyInjection
{
    internal static class ContainerConfig
    {
        private static volatile IContainer _container;
        private static readonly object Lock = new object();

        internal static IContainer Container => _container;

        public static IContainer ConfigureDependencies(Assembly[] additionalAssemblies)
        {
            if (null != _container) return _container;

            lock (Lock)
            {
                if (null != _container) return _container;

                var builder = new ContainerBuilder();

                builder.RegisterApiControllers(typeof(ContainerConfig).Assembly);
                builder.RegisterApiControllers(additionalAssemblies);
                builder.RegisterModule<LoggingModule>();
                builder.RegisterModule<CqrsConfigurationModule>();

                builder.RegisterAssemblyModules(additionalAssemblies);
                builder.Register<SynchronizationContext>(c => SynchronizationContext.Current).InstancePerRequest();


                string connectionString = ConfigurationManager.ConnectionStrings["default"].ConnectionString;
                builder.Register(
                    ctx => new SqlConnection(connectionString))
                    .As<IDbConnection>()
                    .InstancePerRequest();

                builder.RegisterType<ExternalAuthService>().As<IAuthService>().InstancePerRequest();
                builder.RegisterType<AuthDataService>().As<IAuthDataService>().InstancePerRequest();
                builder.RegisterType<FilterHelper>().SingleInstance();

                builder.RegisterType<AuthUserProvider>().As<IAuthUserProvider>().InstancePerRequest();
                
                builder.RegisterModule(new ValidationModule(additionalAssemblies));

                var container = builder.Build();

                _container = container;

                
                return container;
            }
        }
    }
}
