using Game.GOAP;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class SpawnData
{
    [SerializeField] private GameObject enemyPrefab;
    public GameObject EnemyPrefab => enemyPrefab;
    [SerializeField] private GameObject routePrefab;
    public GameObject RoutePrefab => routePrefab;
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
        {
            var enemy = EnemyFactory.SpawnEnemy(data.EnemyPrefab, data.RoutePrefab.transform);
            var patrolBehaviour = enemy.GetComponent<DataPatrolBehaviour>();
            var route = data.RoutePrefab.GetComponent<PatrolRoute>();
            var agent = enemy.GetComponent<NavMeshAgent>();
            patrolBehaviour.SetRawRoute(route);
            agent.Warp(route.Waypoints[0].transform.position);
        }
    }
}