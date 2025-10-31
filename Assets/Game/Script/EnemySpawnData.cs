using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject enemyPrefab;
}
