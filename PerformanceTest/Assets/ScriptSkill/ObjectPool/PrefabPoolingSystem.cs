using System;
using System.Collections.Generic;
using UnityEngine;

public static class PrefabPoolingSystem
{
    private static Dictionary<GameObject, PrefabPool> _prefabToPoolMap = new Dictionary<GameObject, PrefabPool>();
    private static Dictionary<GameObject, PrefabPool> _goToPoolMap = new Dictionary<GameObject, PrefabPool>();

    private static Transform _root;
    public static Transform Root
    {
        get
        {
            if (_root == null)
            {
                _root = new GameObject("Pool").transform;
            }

            return _root;
        }
    }

    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (!_prefabToPoolMap.ContainsKey(prefab))
            _prefabToPoolMap.Add(prefab, new PrefabPool());

        PrefabPool pool = _prefabToPoolMap[prefab];
        GameObject go = pool.Spawn(prefab, position, rotation, parent);
        _goToPoolMap.Add(go, pool);
        return go;
    }

    public static GameObject Spawn(GameObject prefab, Transform parent)
    {
        return Spawn(prefab, Vector3.zero, Quaternion.identity, parent);
    }

    public static bool Despawn(GameObject go)
    {
        if (!_goToPoolMap.ContainsKey(go))
        {
            return false;
        }

        PrefabPool pool = _goToPoolMap[go];
        if (pool.Despawn(go))
        {
            _goToPoolMap.Remove(go);
            return true;
        }

        return false;
    }

    public static void Prespawn(GameObject prefab, int numToSpawn)
    {
        List<GameObject> spawnedObjects = new List<GameObject>();

        for (int i = 0; i < numToSpawn; i++)
        {
            spawnedObjects.Add(Spawn(prefab, Root));
        }

        for (int i = 0; i < numToSpawn; i++)
        {
            Despawn(spawnedObjects[i]);
        }

        spawnedObjects.Clear();
    }

    public static void Reset()
    {
        _prefabToPoolMap.Clear();
        _goToPoolMap.Clear();
    }
}

public struct PoolablePrefabData
{
    public GameObject go;
    public IPoolableComponent[] poolableComponents;
}
public class PrefabPool
{
    private Dictionary<GameObject, PoolablePrefabData> _activeList = new Dictionary<GameObject, PoolablePrefabData>();

    private Queue<PoolablePrefabData> _inactiveList = new Queue<PoolablePrefabData>();

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        PoolablePrefabData data;
        if (_inactiveList.Count > 0)
        {
            data = _inactiveList.Dequeue();
        }
        else
        {
            data = new PoolablePrefabData();

            GameObject newGo = GameObject.Instantiate(prefab, position, rotation, PrefabPoolingSystem.Root);
            data.go = newGo;
            data.poolableComponents = newGo.GetComponents<IPoolableComponent>();
        }

        data.go.SetActive(true);
        data.go.transform.SetParent(parent);
        data.go.transform.position = position;
        data.go.transform.rotation = rotation;

        for (int i = 0; i < data.poolableComponents.Length; i++)
        {
            data.poolableComponents[i].Spawned();
        }

        _activeList.Add(data.go, data);

        return data.go;
    }

    public bool Despawn(GameObject go)
    {
        if (!_activeList.ContainsKey(go))
        {
            return false;
        }

        PoolablePrefabData data = _activeList[go];

        for (int i = 0; i < data.poolableComponents.Length; i++)
        {
            data.poolableComponents[i].Despawned();
        }

        data.go.SetActive(false);
        data.go.transform.SetParent(PrefabPoolingSystem.Root);
        _activeList.Remove(go);
        _inactiveList.Enqueue(data);
        return true;
    }

}
