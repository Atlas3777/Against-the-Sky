using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PointOfInterest : MonoBehaviour
{
    public ActionSpot[] AllWayPoints;
    
    public GameObject Enemy;

    public Transform[] EnemySpawnPoints;

    public List<IEnemy> AllEnemy = new();

    public void StartPointOfInterest()
    {
        foreach (var spawnPoint in EnemySpawnPoints)
        {
            var e = EnemyFactory.SpawnEnemy(Enemy, spawnPoint);
            AllEnemy.Add(e);
        }
    }

    public void Agree()
    {
        foreach (var e in AllEnemy)
        {
            
        }        
    }
    
    
    public ActionSpot GetFirstActionSpot()
    {
        return AllWayPoints.FirstOrDefault(x => !x.IsOccupied);
    }
    
    public List<ActionSpot> GetAllAvailableActionSpot()
    {
        return AllWayPoints.Where(x => !x.IsOccupied).ToList();
    }
}