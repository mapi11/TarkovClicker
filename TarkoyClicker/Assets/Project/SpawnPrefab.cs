using UnityEngine;

public class SpawnPrefab : MonoBehaviour
{
    [SerializeField] private GameObject _spawnPrefab;
    [SerializeField] private Transform _prefabParent;

    private void Awake()
    {
        Instantiate(_spawnPrefab, _prefabParent.transform);
    }

}
