using System.Reflection;
using Autofac;
using FluentValidation;
using Module = Autofac.Module;

namespace Validation
{
    public class ValidationModule : Module
    {
        private readonly Assembly[] _assembliesToQuery;

        public ValidationModule(Assembly[] assembliesToQuery)
        {
            _assembliesToQuery = assembliesToQuery;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(_assembliesToQuery)
                .Where(t => typeof(IValidator).IsAssignableFrom(t))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<AutofacValidatorFactory>().As<IValidatorFactory>().SingleInstance();

            builder.RegisterBuildCallback(OnContainerConfigured);

            base.Load(builder);
        }

        private void OnContainerConfigured(IContainer container)
        {
            ValidationExtensions.ValidatorFactory = container.Resolve<IValidatorFactory>();
        }
    }
}