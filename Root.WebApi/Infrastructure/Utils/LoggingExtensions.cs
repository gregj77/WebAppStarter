using System;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using NLog;

namespace Utils
{
    public static class LoggingExtensions
    {
        public static IObservable<T> LogDebugMsg<T>(this IObservable<T> stream, ILogger logger, string message = null, [CallerMemberName]string methodName = null)
        {
            if (!logger.IsDebugEnabled) return stream;

            return stream.Do(item =>
            {
                logger.Debug($"{methodName ?? string.Empty}({item}) {message?? string.Empty}");
            });
        }

        public static IObservable<T> LogDebug<T>(this IObservable<T> stream, ILogger logger, Func<T, string> messageFunc = null, [CallerMemberName]string methodName = null)
        {
            if (!logger.IsDebugEnabled) return stream;

            return stream.Do(item =>
            {
                logger.Debug($"{methodName ?? string.Empty} {messageFunc?.Invoke(item)}");
            });
        }

        public static IObservable<T> LogInfoMsg<T>(this IObservable<T> stream, ILogger logger, string message = null, [CallerMemberName]string methodName = null)
        {
            if (!logger.IsInfoEnabled) return stream;

            return stream.Do(item =>
            {
                logger.Info($"{methodName ?? string.Empty}({item}) {message ?? string.Empty}");
            });
        }

        public static IObservable<T> LogInfo<T>(this IObservable<T> stream, ILogger logger, Func<T, string> messageFunc = null, [CallerMemberName]string methodName = null)
        {
            if (!logger.IsInfoEnabled) return stream;

            return stream.Do(item =>
            {
                logger.Info($"{methodName ?? string.Empty} {messageFunc?.Invoke(item)}");
            });
        }

        public static IObservable<T> LogWarnMsg<T>(this IObservable<T> stream, ILogger logger, string message = null, [CallerMemberName]string methodName = null)
        {
            if (!logger.IsWarnEnabled) return stream;
            return stream.Do(item =>
            {
                logger.Warn($"{methodName ?? string.Empty}({item}) {message ?? string.Empty}");
            });
        }

        public static IObservable<T> LogWarn<T>(this IObservable<T> stream, ILogger logger, Func<T, string> messageFunc = null, [CallerMemberName]string methodName = null)
        {
            if (!logger.IsWarnEnabled) return stream;
            return stream.Do(item =>
            {
                logger.Warn($"{methodName ?? string.Empty} {messageFunc?.Invoke(item)}");
            });
        }

        public static IObservable<T> LogErrorMsg<T>(this IObservable<T> stream, ILogger logger, string message = null, [CallerMemberName]string methodName = null)
        {
            return stream.Do(item =>
            {
                logger.Error($"{methodName ?? string.Empty}({item}) {message ?? string.Empty}");
            });
        }

        public static IObservable<T> LogError<T>(this IObservable<T> stream, ILogger logger, Func<T, string> messageFunc = null, [CallerMemberName]string methodName = null)
        {
            return stream.Do(item =>
            {
                logger.Error($"{methodName ?? string.Empty} {messageFunc?.Invoke(item)}");
            });
        }

        public static IObservable<T> LogException<T>(this IObservable<T> stream, ILogger logger,
            Func<Exception, string> messageFunc = null, [CallerMemberName] string methodName = null)
        {
            return stream.Do(_ => { }, err =>
            {
                string msg = messageFunc?.Invoke(err);
                msg = msg ?? $"{err.Message}<{err.GetType().FullName}>\n{err.StackTrace}";
                logger.Warn($"{methodName} - {msg}");
            });
        } 
    }
}
