using System;
using System.Reflection;
using Api.Setup;
using Feature01;

namespace SomeApi
{
    public class WebApiApplication : HttpApplicationBase
    {
        protected override Assembly[] OnConfigureModules()
        {
            return new[] { typeof(WebApiApplication).Assembly, typeof(Feature01Module).Assembly };
        }

        protected override bool CanConfigureAssembly(string assemblyName)
        {
            return assemblyName.StartsWith("SOMEAPI", StringComparison.OrdinalIgnoreCase);
        }
    }
}
