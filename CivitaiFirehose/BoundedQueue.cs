using System.Collections;

namespace CivitaiFirehose;

public class BoundedQueue<T>(int maxSize) : IEnumerable<T>
{
    private readonly Queue<T> _queue = new(maxSize);

    public void Enqueue(T item)
    {
        if (_queue.Count == maxSize)
        {
            _queue.Dequeue();
        }

        _queue.Enqueue(item);
    }

    public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _queue.GetEnumerator();
}