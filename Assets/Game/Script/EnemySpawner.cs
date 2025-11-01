using UnityEngine;

[System.Serializable]
public class SpawnData
{
    [SerializeField] private Transform spawnPoint;
    public Transform SpawnPoint => spawnPoint;
    [SerializeField] private GameObject enemyPrefab;
    public GameObject EnemyPrefab => enemyPrefab;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private SpawnData[] spawns;

    void Start()
    {
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        foreach (var data in spawns)
            EnemyFactory.SpawnEnemy(data.EnemyPrefab, data.SpawnPoint);
    }
}