namespace Boolkit;

public class AsyncSemaphore
{
    private readonly SemaphoreSlim _semaphore;

    private int _count = 0;

    public int Count => _count;

    public AsyncSemaphore(int maxConcurrency = 1) => _semaphore = new(maxConcurrency, maxConcurrency);

    public async Task<IDisposable> EnterAsync()
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        return Disposable.Create(() => _semaphore.Release());
    }

    public async Task<IDisposable> EnterAndCountAsync()
    {
        Interlocked.Increment(ref _count);

        await _semaphore.WaitAsync().ConfigureAwait(false);

        return Disposable.Create(() =>
        {
            _semaphore.Release();

            Interlocked.Decrement(ref _count);
        });
    }
}
