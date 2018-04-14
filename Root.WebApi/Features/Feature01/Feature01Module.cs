using Autofac;
using Feature01.Data;
using Feature01.Mapping;
using Feature01.Services;
using Feature01.Services.Implementation;
using Quarks.CQRS;

namespace Feature01
{
    public class Feature01Module : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new Mappings());

            builder.RegisterType<DeviceService>().As<IDeviceService>().InstancePerRequest();
            builder.RegisterType<DataService>().As<IDataService>().InstancePerRequest();

            builder.RegisterAssemblyTypes(typeof(Feature01Module).Assembly)
                .AsClosedTypesOf(typeof(ICommandHandler<>))
                .AsImplementedInterfaces()
                .InstancePerRequest();

            builder.RegisterAssemblyTypes(typeof(Feature01Module).Assembly)
                .AsClosedTypesOf(typeof(IQueryHandler<,>))
                .AsImplementedInterfaces()
                .InstancePerRequest();

            base.Load(builder);
        }
    }
}
