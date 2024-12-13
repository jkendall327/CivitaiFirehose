using System.Collections;

namespace CivitaiFirehose;

public class BoundedQueue<T>(int maxSize) : IEnumerable<T>
{
    private readonly LinkedList<T> _list = [];

    public void Enqueue(T item)
    {
        _list.AddFirst(item);

        if (_list.Count > maxSize)
        {
            _list.RemoveLast();
        }
    }
    
    public void Clear() => _list.Clear();
    
    public IReadOnlyList<T> AsReadOnly() => new List<T>(_list).AsReadOnly();

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
}