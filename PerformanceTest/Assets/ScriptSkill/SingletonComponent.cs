using UnityEngine;

public class SingletonComponent<T> : MonoBehaviour where T : SingletonComponent<T>
{
    private static T _instance;

    //原先书中样例代码：protected static SingletonComponent<T> instance，麻烦！需要子类显式声明Instance属性，增加了代码量。让子类提供了更多的控制策略、更灵活，但不统一。

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                T[] managers = GameObject.FindObjectsByType(typeof(T), FindObjectsSortMode.None) as T[];
                if (managers != null)
                {
                    if (managers.Length == 1)
                    {
                        _instance = managers[0];
                        return _instance;
                    }
                    else if (managers.Length > 1)
                    {
                        Debug.LogError($"[Singleton] More then one {typeof(T).Name} in the scene.");
                        for (int i = 0; i < managers.Length; i++)
                        {
                            T manager = managers[i];
                            Destroy(manager.gameObject);
                        }
                    }
                }

                GameObject go = new GameObject(typeof(T).Name, typeof(T));
                _instance = go.GetComponent<T>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
        set
        {
            _instance = value as T;
        }
    }

    private bool _alive = true;

    public static bool IsAlive
    {
        get
        {
            if (_instance == null) return false;
            return _instance._alive;
        }
    }
    private void OnDestroy()
    {
        _alive = false;
    }

    private void OnApplicationQuit()
    {
        _alive = false;
    }

}
