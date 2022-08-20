namespace Boolkit;

public record ResultUnion<T>
{
    public T? Result { get; init; }
    public bool Error { get; init; } = false;
}

public record ResultUnion<T, U>
{
    public T? Result { get; init; }
    public U? ErrorType { get; init; }
}
