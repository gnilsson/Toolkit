using System.Collections.ObjectModel;

namespace GN.Toolkit;

public static class FunctionsCache
{
    public const string EnvironmentVariableCacheTracking = "FunctionsCacheTracking";

    static FunctionsCache()
    {
        var tracking = Environment.GetEnvironmentVariable(EnvironmentVariableCacheTracking);

        if (tracking is not null && bool.TryParse(tracking, out var toggle) && toggle)
        {
            var directoryDictionary = new Dictionary<Guid, Directory>();
            AddToDirectoryDictionary = Operations.AddTo(directoryDictionary);
            UpdateDirectoryDictionary = Operations.UpdateTo(directoryDictionary);
            GetDirectory = Operations.GetDirectory(directoryDictionary);
            var readOnly = new ReadOnlyDictionary<Guid, Directory>(directoryDictionary);
            GetDirectoryDictionary = () => readOnly;
        }
    }

    internal static Func<ReadOnlyDictionary<Guid, Directory>> GetDirectoryDictionary { get; } = default!;
    internal static Func<Guid, Directory> GetDirectory { get; } = default!;
    private static Action<Guid, Directory>? AddToDirectoryDictionary { get; } = default!;
    private static Action<Guid, Directory>? UpdateDirectoryDictionary { get; } = default!;

    internal static class Operations
    {
        internal delegate bool Try<TU>(TU input, out TU output);

        internal static Try<Dictionary<TKey, TOut>> TryClear<TKey, TOut>(Directory cacheDirectory) where TKey : notnull
        {
            return (Dictionary<TKey, TOut> input, out Dictionary<TKey, TOut> output) =>
            {
                if (cacheDirectory.Contains(typeof(TKey)))
                {
                    output = input;
                    return false;
                }

                output = new();
                return true;
            };
        }

        internal static Action<Type> AddTo(Directory cacheDirectory)
            => (Type type) => cacheDirectory.Add(type);
        internal static Action<Type, Directory> AddTo(Dictionary<Type, Directory> directoryDictionary)
            => (Type key, Directory value) => directoryDictionary.Add(key, value);
        internal static Action<Guid, Directory> AddTo(Dictionary<Guid, Directory> directoryDictionary)
            => (Guid key, Directory value) => directoryDictionary.Add(key, value);
        internal static Action<Guid, Directory> UpdateTo(Dictionary<Guid, Directory> directoryDictionary)
            => (Guid key, Directory value) => directoryDictionary[key] = value;
        internal static Func<Guid, Directory> GetDirectory(Dictionary<Guid, Directory> directoryDictionary)
            => (Guid key) => directoryDictionary[key];
    }

    private class Responsibility<TIn, TOut> where TIn : notnull
    {
        public Responsibility(Guid directoryKey)
        {
            var cacheDirectory = GetDirectory(directoryKey);

            TryClearDirectory = Operations.TryClear<TIn, TOut>(cacheDirectory);

            AddToDirectory = Operations.AddTo(cacheDirectory);
        }

        internal Operations.Try<Dictionary<TIn, TOut>> TryClearDirectory { get; }

        internal Action<Type> AddToDirectory { get; }
    }

    public class Directory
    {
        private readonly List<Type> _values = new();
        private readonly Guid _key;

        public Directory()
        {
            _key = Guid.NewGuid();
            AddToDirectoryDictionary!(_key, this);
        }

        public Guid Key => _key;

        public void ClearAll() => _values.Clear();

        internal void Add(Type type)
        {
            _values.Add(type);
            UpdateDirectoryDictionary!(_key, this);
        }

        internal bool Contains(Type type) => _values.Contains(type);
    }

    public static Func<TKey, UIn, TOut> Memoise<TKey, UIn, TOut>(Func<TKey, UIn, TOut> func, Guid directoryKey) where TKey : notnull
    {
        var responsibility = new Responsibility<TKey, TOut>(directoryKey);

        var cache = new Dictionary<TKey, TOut>();

        return (inputT, inputU) =>
        {
            if (responsibility.TryClearDirectory(cache, out cache))
            {
                responsibility.AddToDirectory(typeof(TKey));
            }
            else if (cache.TryGetValue(inputT, out var result)) return result;

            return cache[inputT] = func(inputT, inputU);
        };
    }

    public static Func<TKey, CancellationToken, TOut> Memoise<TKey, TOut>(Func<TKey, CancellationToken, TOut> func, Guid directoryKey) where TKey : notnull
    {
        var responsibility = new Responsibility<TKey, TOut>(directoryKey);

        var cache = new Dictionary<TKey, TOut>();

        return (input, ct) =>
        {
            lock (responsibility)
            {
                if (responsibility.TryClearDirectory(cache, out cache))
                {
                    responsibility.AddToDirectory(typeof(TKey));
                }
                else if (cache.TryGetValue(input, out var result)) return result;

                return cache[input] = func(input, ct);
            }
        };
    }

    public static Func<TKey, TOut> Memoise<TKey, TOut>(Func<TKey, TOut> func, Guid directoryKey) where TKey : notnull
    {
        var responsibility = new Responsibility<TKey, TOut>(directoryKey);

        var cache = new Dictionary<TKey, TOut>();

        return input =>
        {
            if (responsibility.TryClearDirectory(cache, out cache))
            {
                responsibility.AddToDirectory(typeof(TKey));
            }
            else if (cache.TryGetValue(input, out var result)) return result;

            return cache[input] = func(input);
        };
    }

    public static Func<TKey, UIn, TOut> Memoise<TKey, UIn, TOut>(Func<TKey, UIn, TOut> func) where TKey : notnull
    {
        var cache = new Dictionary<TKey, TOut>();

        return (inputT, inputU) =>
        {
            if (cache.TryGetValue(inputT, out var result)) return result;

            return cache[inputT] = func(inputT, inputU);
        };
    }

    public static Func<TKey, TOut> Memoise<TKey, TOut>(Func<TKey, TOut> func) where TKey : notnull
    {
        var cache = new Dictionary<TKey, TOut>();

        return input =>
        {
            if (cache.TryGetValue(input, out var result)) return result;

            return cache[input] = func(input);
        };
    }

    public static Func<TKey, CancellationToken, TResult> AsyncMemoise<TKey, TResult>(Func<TKey, CancellationToken, TResult> func) where TKey : notnull
    {
        var cache = new Dictionary<TKey, TResult>();

        return (input, ct) =>
        {
            lock (cache)
            {
                if (cache.TryGetValue(input, out var result)) return result;

                return cache[input] = func(input, ct);
            }
        };
    }

    public static Func<TKey, TParam, CancellationToken, TResult> AsyncMemoise<TKey, TParam, TResult>(Func<TKey, TParam, CancellationToken, TResult> func) where TKey : notnull
    {
        var cache = new Dictionary<TKey, TResult>();

        return (inputKey, inputParam, ct) =>
        {
            lock (cache)
            {
                if (cache.TryGetValue(inputKey, out var result)) return result;

                return cache[inputKey] = func(inputKey, inputParam, ct);
            }
        };
    }
}
