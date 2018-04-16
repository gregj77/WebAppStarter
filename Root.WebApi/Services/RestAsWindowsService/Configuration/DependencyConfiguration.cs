using Api.Setup;
using Autofac;
using System.Web.Http;

namespace RestAsWindowsService.Configuration
{
    public class DependencyConfiguration : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RestServiceStarter>();

            var httpConfig = new HttpConfiguration();
            builder.RegisterInstance(httpConfig);

            builder.RegisterBuildCallback(container => WebApiConfiguration.ConfigureWebApi(httpConfig, container));

            base.Load(builder);
        }


    }
}
