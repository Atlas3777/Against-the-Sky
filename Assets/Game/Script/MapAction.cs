using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapAction : MonoBehaviour
{
    [SerializeField] private ActionPOI[] AllPOI;
    [SerializeField] private GameObject Enemy;
    [SerializeField] private Transform[] EnemySpawnPoints;
    [SerializeField] private List<GameObject> AllEnemy = new();
    [SerializeField] private GameObject Reward;
    
    [SerializeField] private float scanRadius = 20f;
    [SerializeField] private float activateRadius = 50f;
    
    public float ScanRadius => scanRadius;
    public float ActivateRadius => activateRadius;
    
    private bool _running;
    public bool IsRunning => _running;
    
    private int _aliveEnemiesCount;

    public void RegisterEnemyDeath()
    {
        _aliveEnemiesCount--;
        if (_aliveEnemiesCount <= 0)
        {
            OnAllEnemiesDead();
        }
    }

    private void OnAllEnemiesDead()
    {
        Debug.Log("Все враги убиты! Создаём награду.");
        Reward.SetActive(true);
    }

    public void StartPointOfInterest()
    {
        Debug.Log("Starting Point of Interest");
        _running = true;
        foreach (var spawnPoint in EnemySpawnPoints)
        {
            //var e = EnemyFactory.SpawnEnemy(Enemy, spawnPoint, this);
            //AllEnemy.Add(e);
        }
    }

    public ActionPOI GetNearFreePOI(Transform body)
    {
        return AllPOI
            .Where(poi => poi.IsAvailable)
            .OrderBy(poi => Vector3.Distance(poi.BodyPositionTarget.position, body.position))
            .FirstOrDefault();
    }

    public void ScanPOIs()
    {
        var pois = FindObjectsByType<ActionPOI>(0)
            .Where(poi => Vector3.Distance(poi.transform.position, transform.position) <= scanRadius)
            .ToArray();

        AllPOI = pois;
        Debug.Log($"Найдено {AllPOI.Length} ActionPOI в радиусе {scanRadius}");
    }
}