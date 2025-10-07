using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapAction : MonoBehaviour
{
    public ActionPOI[] AllPOI;

    public Transform sparePoint;
    
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
    
    
    public ActionPOI GetFirstActionSpot()
    {
        return AllPOI.FirstOrDefault(x => !x.IsOccupied);
    }
    
    public List<ActionPOI> GetAllAvailableActionSpot()
    {
        return AllPOI.Where(x => !x.IsOccupied).ToList();
    }
    
    public ActionPOI GetNearFreePOI(Transform body)
    {
        return AllPOI
            .Where(poi => !poi.IsOccupied)
            .OrderBy(poi => Vector3.Distance(poi.BodyPositionTarget.position, body.position))
            .FirstOrDefault();
    }
}