using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-9998)]
public class SpawnPointManager : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPoints;
    
    private void Awake()
    {
        G.SpawnPointManager = this;
    }

    public Transform GetSpawnPosition()
    {
        return spawnPoints.FirstOrDefault()?.transform;
    }
}