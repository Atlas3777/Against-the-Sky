using System.Collections.Generic;
using UnityEngine;

public static class EnemyFactory
{
    public static IEnemy SpawnEnemy(GameObject EnemyPrefab, Transform EnemySpawnPoint)
    {
        var enemy = Object.Instantiate(EnemyPrefab, EnemySpawnPoint.position, Quaternion.identity);
        if (enemy.TryGetComponent<IEnemy>(out var controller))
        {
            controller.GetComponents();
            controller.Init(G.Player);
            controller.MyStart();
        }

        return controller;
    }
    // public static GameObject SpawnEnemyWithPoint(GameObject EnemyPrefab, Transform EnemySpawnPoint, List<ActionSpot> ActionSpots)
    // {
    //     var e = SpawnEnemy(EnemyPrefab, EnemySpawnPoint);
    //     
    // }
}