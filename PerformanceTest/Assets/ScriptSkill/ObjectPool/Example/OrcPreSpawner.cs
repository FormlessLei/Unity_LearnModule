using UnityEngine;

public class OrcPreSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _orcPrefab;
    [SerializeField] private int _numToSpawn = 20;

    private void Start()
    {
        PrefabPoolingSystem.Prespawn(_orcPrefab, _numToSpawn);
    }

}
