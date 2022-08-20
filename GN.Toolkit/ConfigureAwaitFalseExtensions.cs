using System.Runtime.CompilerServices;

namespace Boolkit;

public static class ConfigureAwaitFalseExtensions
{
    public static ConfiguredTaskAwaitable CAF(this Task task) => task.ConfigureAwait(false);
    public static ConfiguredTaskAwaitable<T> CAF<T>(this Task<T> task) => task.ConfigureAwait(false);
    public static ConfiguredValueTaskAwaitable CAF(this ValueTask task) => task.ConfigureAwait(false);
    public static ConfiguredValueTaskAwaitable<T> CAF<T>(this ValueTask<T> task) => task.ConfigureAwait(false);
    public static ConfiguredCancelableAsyncEnumerable<T> CAF<T>(this ConfiguredCancelableAsyncEnumerable<T> task) => task.ConfigureAwait(false);
    public static ConfiguredCancelableAsyncEnumerable<T> CAF<T>(this IAsyncEnumerable<T> task) => task.ConfigureAwait(false);
}
