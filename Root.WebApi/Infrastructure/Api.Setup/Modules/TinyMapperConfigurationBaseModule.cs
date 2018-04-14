using Autofac;
using Autofac.Core;

namespace Api.Setup.Modules
{
    public abstract class TinyMapperConfigurationBaseModule : Module
    {
        protected sealed override void Load(ContainerBuilder builder)
        {
            RegisterMappings(builder);
            base.Load(builder);
        }

        protected sealed override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            base.AttachToComponentRegistration(componentRegistry, registration);
        }

        protected sealed override void AttachToRegistrationSource(IComponentRegistry componentRegistry, IRegistrationSource registrationSource)
        {
            base.AttachToRegistrationSource(componentRegistry, registrationSource);
        }

        protected abstract void RegisterMappings(ContainerBuilder builder);
    }
}
