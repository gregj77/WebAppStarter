using System.Runtime.CompilerServices;

namespace Utils
{
    public static class Cast
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T To<T>(dynamic instance)
        {
            return (T)instance;
        }
    }
}
