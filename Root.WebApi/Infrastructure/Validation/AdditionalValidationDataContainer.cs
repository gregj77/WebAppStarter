using System.Runtime.CompilerServices;

namespace Validation
{
    internal static class AdditionalValidationDataContainer
    {
        private static readonly ConditionalWeakTable<object, object> Storage = new ConditionalWeakTable<object, object>();

        public static void Add(object key, object value)
        {
            Storage.Remove(key);
            Storage.Add(key, value);
        }

        public static TResult Get<TResult>(object key)
        {
            return (TResult)Storage.GetOrCreateValue(key);
        }
    }
}