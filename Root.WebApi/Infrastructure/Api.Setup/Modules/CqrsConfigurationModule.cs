using System;
using Autofac;
using Quarks.CQRS;
using Quarks.CQRS.Impl;

namespace Api.Setup.Modules
{
    public class CqrsConfigurationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<HandlerFactory>()
                .As<ICommandHandlerFactory>()
                .As<IQueryHandlerFactory>()
                .InstancePerDependency();

            builder.RegisterType<RxCommandDispatcher>()
                .As<ICommandDispatcher>()
                .As<IRxCommandDispatcher>()
                .InstancePerDependency();

            builder.RegisterType<RxQueryDispatcher>()
                .As<IQueryDispatcher>()
                .As<IRxQueryDispatcher>()
                .InstancePerDependency();   

            builder.RegisterAssemblyTypes(typeof(CqrsConfigurationModule).Assembly)
                .AsClosedTypesOf(typeof(ICommandHandler<>))
                .AsImplementedInterfaces()
                .InstancePerRequest();

            builder.RegisterAssemblyTypes(typeof(CqrsConfigurationModule).Assembly)
                .AsClosedTypesOf(typeof(IQueryHandler<,>))
                .AsImplementedInterfaces()
                .InstancePerRequest();

            base.Load(builder);
        }

        private class HandlerFactory : ICommandHandlerFactory, IQueryHandlerFactory
        {
            private readonly IComponentContext _container;

            public HandlerFactory(IComponentContext container)
            {
                _container = container;
            }

            object ICommandHandlerFactory.CreateHandler(Type handlerType)
            {
                return _container.Resolve(handlerType);
            }

            object IQueryHandlerFactory.CreateHandler(Type handlerType)
            {
                return _container.Resolve(handlerType);
            }
        }
    }
}