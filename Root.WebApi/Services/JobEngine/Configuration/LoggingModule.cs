using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using NLog;
using Module = Autofac.Module;

namespace JobEngine.Configuration
{
    internal class LoggingModule : Module
    {
        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            registration.Preparing += OnComponentPreparing;
            registration.Activated += OnComponentActivated;
        }

        private void OnComponentActivated(object sender, ActivatedEventArgs<object> e)
        {
            var instance = e.Instance;
            var instanceType = instance.GetType();

            // Get all the injectable properties to set.
            // If you wanted to ensure the properties were only UNSET properties,
            // here's where you'd do it.
            var properties = instanceType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(ILogger) && p.CanWrite && p.GetIndexParameters().Length == 0);


            // Set the properties located.
            foreach (var propToSet in properties)
            {
                var logger = LogManager.GetLogger(instanceType.Name);
                propToSet.SetValue(instance, logger, null);
            }
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => LogManager.GetLogger("App"))
                .As<ILogger>();

            base.Load(builder);
        }

        private void OnComponentPreparing(object sender, PreparingEventArgs e)
        {
            e.Parameters = e.Parameters.Union(
                new[]
                {
                    new ResolvedParameter(
                        (p, i) => p.ParameterType == typeof (ILogger),
                        (p, i) => LogManager.GetLogger(p.Member.DeclaringType.Name)
                    ),
                });
        }
    }
}
