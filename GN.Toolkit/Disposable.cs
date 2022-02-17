namespace GN.Toolkit;

public class Disposable : IDisposable
{
    private Action? _onDispose;

    Disposable(Action onDispose) => _onDispose = onDispose;

    public static IDisposable Create(Action onDispose) => new Disposable(onDispose);

    public void Dispose()
    {
        Action todo;

        lock (this)
        {
            todo = _onDispose!;
            _onDispose = default;
        }

        todo?.Invoke();
        GC.SuppressFinalize(this);
    }
}
