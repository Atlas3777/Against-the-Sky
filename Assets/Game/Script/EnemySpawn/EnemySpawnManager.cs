using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] private List<EnemyTypeTuple> enemyTypeTuple;

    private Dictionary<EnemyType, GameObject> enemyTypeDictionary;

    private void Start()
    {
        enemyTypeDictionary = enemyTypeTuple.ToDictionary(t => t.Type, t => t.GameObject);
        var t = G.Timer;
        print("Start: "+Time.time);
        t.AddTimerWithCountDown(() => { print("Таймер закончился"); },10, () => { print(Time.time); },0.25f);
    }

    [Serializable]
    public enum EnemyType
    {
        First,
        Second
    }

    [Serializable]
    public struct EnemyTypeTuple
    {
        public EnemyType Type;
        public GameObject GameObject;
    }
}
