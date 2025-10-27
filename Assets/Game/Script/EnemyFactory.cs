using System;
using System.Collections.Generic;
using Game.GOAP;
using Game.GOAP.Behaviours;
using UnityEngine;
using Object = UnityEngine.Object;

public static class EnemyFactory
{
    public static GameObject SpawnEnemy(GameObject enemyPrefab, Transform enemySpawnPoint)
    {
        var enemy = Object.Instantiate(enemyPrefab, enemySpawnPoint.position, Quaternion.identity);
        if (enemy.TryGetComponent<IEnemy>(out var controller))
        {
            controller.GetComponents();
            controller.Init(G.Player);
            controller.MyStart();
        }

        return enemy;
    }
    public static GameObject SpawnEnemy(GameObject enemyPrefab, Transform enemySpawnPoint, MapAction mapAction)
    {
        var enemy = Object.Instantiate(enemyPrefab, enemySpawnPoint.position, Quaternion.identity);

        if (enemy.TryGetComponent<BrainBehaviour>(out var brainBehaviour))
            brainBehaviour.MyAwake();

        if(enemy.TryGetComponent<DataPatrolBehaviour>(out var behaviour))
            behaviour.MapAction = mapAction;
        
        return enemy;
    }
}