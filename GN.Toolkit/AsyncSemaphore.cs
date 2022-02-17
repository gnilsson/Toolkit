namespace GN.Toolkit;

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

    public async Task<IDisposable> EnterWithCountAsync()
    {
        _count++;

        await _semaphore.WaitAsync().ConfigureAwait(false);

        return Disposable.Create(() =>
        {
            _semaphore.Release();
            _count--;
        });
    }
}
