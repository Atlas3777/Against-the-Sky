using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapAction : MonoBehaviour
{
    public float scanRadius = 20f; // Добавлено для редактора
    public float runningRadius = 50f;
    
    public ActionPOI[] AllPOI;
    public GameObject Enemy;
    public Transform[] EnemySpawnPoints;
    public List<GameObject> AllEnemy = new();
    public GameObject Reward;
    

    private bool _running;
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

    public void Update()
    {
        if(_running)
            return;
        
        if(Vector3.Distance(G.Player.transform.position, this.transform.position) <= runningRadius)
            StartPointOfInterest();
    }

    public void StartPointOfInterest()
    {
        Debug.Log("Starting Point of Interest");
        _running = true;
        foreach (var spawnPoint in EnemySpawnPoints)
        {
            var e = EnemyFactory.SpawnEnemy(Enemy, spawnPoint, this);
            AllEnemy.Add(e);
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
        var pois = FindObjectsOfType<ActionPOI>()
            .Where(poi => Vector3.Distance(poi.transform.position, transform.position) <= scanRadius)
            .ToArray();

        AllPOI = pois;
        Debug.Log($"Найдено {AllPOI.Length} ActionPOI в радиусе {scanRadius}");
    }
}