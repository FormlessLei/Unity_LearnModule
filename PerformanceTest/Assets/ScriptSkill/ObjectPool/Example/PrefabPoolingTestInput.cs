using System.Collections.Generic;
using UnityEngine;

public class PrefabPoolingTestInput : MonoBehaviour
{
    [SerializeField] private GameObject _orcPrefab;
    [SerializeField] private GameObject _trollPrefab;
    [SerializeField] private GameObject _ogrePrefab;
    [SerializeField] private GameObject _dragonPrefab;

    private List<GameObject> _orcs = new List<GameObject>();
    private List<GameObject> _trolls = new List<GameObject>();
    private List<GameObject> _ogres = new List<GameObject>();
    private List<GameObject> _dragons = new List<GameObject>();

    private void Start()
    {
        PrefabPoolingSystem.Prespawn(_orcPrefab, 11);
        PrefabPoolingSystem.Prespawn(_trollPrefab, 8);
        PrefabPoolingSystem.Prespawn(_ogrePrefab, 5);
        PrefabPoolingSystem.Prespawn(_dragonPrefab, 1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { SpawnObject(_orcPrefab, _orcs); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { SpawnObject(_trollPrefab, _trolls); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { SpawnObject(_ogrePrefab, _ogres); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { SpawnObject(_dragonPrefab, _dragons); }

        if (Input.GetKeyDown(KeyCode.Q)) { DespawnRandomObject(_orcs); }
        if (Input.GetKeyDown(KeyCode.W)) { DespawnRandomObject(_trolls); }
        if (Input.GetKeyDown(KeyCode.E)) { DespawnRandomObject(_ogres); }
        if (Input.GetKeyDown(KeyCode.R)) { DespawnRandomObject(_dragons); }
    }

    private void SpawnObject(GameObject prefab, List<GameObject> list)
    {
        GameObject obj = PrefabPoolingSystem.Spawn(prefab, 5.0f * Random.insideUnitSphere, Quaternion.identity, null);
        list.Add(obj);
    }

    private void DespawnRandomObject(List<GameObject> list)
    {
        if (list.Count == 0) return;

        int i = Random.Range(0, list.Count);
        PrefabPoolingSystem.Despawn(list[i]);
        list.RemoveAt(i);
    }
}