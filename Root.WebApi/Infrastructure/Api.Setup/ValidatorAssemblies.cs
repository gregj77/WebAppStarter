using System.Reflection;

namespace ePatrol.Api.Setup
{
    internal class ValidatorAssemblies
    {
        private readonly Assembly[] _assemblies;

        internal ValidatorAssemblies(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
            Instance = this;
        }

        public static ValidatorAssemblies Instance { get; private set; }

        public static implicit operator Assembly[](ValidatorAssemblies rhs)
        {
            return rhs._assemblies;
        }
    }
}