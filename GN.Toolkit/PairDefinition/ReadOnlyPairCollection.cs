using System.Collections.ObjectModel;

namespace Boolkit;

public class ReadOnlyPairCollection<T1, T2> : ReadOnlyCollection<Pair<T1, T2>>
{
    private readonly PairCollection<T1, T2> _pairCollection;

    public ReadOnlyPairCollection(PairCollection<T1, T2> pairCollection) : base(pairCollection)
    {
        _pairCollection = pairCollection;
    }

    public T2? this[T1 key] => _pairCollection[key];

    public T1? this[T2 key] => _pairCollection[key];
}
