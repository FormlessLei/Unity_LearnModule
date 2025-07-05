using System.Collections.Generic;

public class ObjectPool<T> where T : IPoolableObject, new()
{
    private Stack<T> _pool;
    // 怀疑此记录的意义。
    private int _currentIndex = 0;

    public int Count => _pool.Count;

    public void Init(int capacity)
    {
        _pool = new Stack<T>(capacity);
        for (int i = 0; i < capacity; i++)
        {
            Spawn();
        }

        Reset();
    }


    public void Reset()
    {
        _currentIndex = 0;
    }

    public T Spawn()
    {
        if (_currentIndex < _pool.Count)
        {
            T result = _pool.Pop();
            _currentIndex++;
            result.Respawn();
            return result;
        }
        else
        {
            T result = new T();
            _pool.Push(result);
            _currentIndex++;
            result.New();
            return result;
        }

    }

}