using System;
using System.Collections.Generic;
using System.Linq;
using cowsins;
using cowsins.SaveLoad;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EvacuationManager : MonoBehaviour
{
    [SerializeField] private List<EnemyTypeTuple> enemyTypeTuple;
    [SerializeField] private List<Transform> enemySpawnPoints;
    [SerializeField] private int firstStageTimeInSeconds;
    [SerializeField] private int nextStagesTimeInSeconds;
    [SerializeField] private int evacuationZoneActiveTimeInSeconds;
    [SerializeField] private int evacuationCountdownTimeInSeconds;
    [SerializeField] private List<EvacuationZoneTrigger> evacuationZoneTriggers;
    [SerializeField] private string SceneToLoad;

    private Dictionary<EnemyType, GameObject> enemyTypeDictionary;
    private List<GameObject> enemies = new();
    private Timer timer;
    private int countdown;
    private int evacuationCountdown;

    private bool isEvacuationZonesActive = false;
    private bool isDamageRecieved = false;
    private bool isEvacuationCountdownStart = false;

    public void OnEvacuationTriggerEnter()
    {
        if (isEvacuationZonesActive)
        {
            evacuationCountdown = evacuationCountdownTimeInSeconds;
            isEvacuationCountdownStart = true;
            timer.AddTimerWithCountDown(LoadScene,evacuationCountdownTimeInSeconds,UpdateZoneTimer,1,StopEvacuationZoneTimer);
        }
    }

    private bool StopEvacuationZoneTimer()
    {
        if (isDamageRecieved)
        {
            isDamageRecieved = false;
            return true;
        }
        return !isEvacuationZonesActive || !isEvacuationCountdownStart;
    }

    private void isDamageRecivedToggle(bool isDamaged) 
    { 
        isDamageRecieved = isDamaged; 
        if (isDamaged)
            OnEvacuationTriggerEnter();
    }

    private void LoadScene()
    {
        if (DataPersistenceManager.instance == null)
        {
            ToastManager.Instance?.ShowToast(ToastManager.Instance?.DataPersistenceManagerNotAvailableMsg);
            Debug.Log("<color=red>[COWSINS]</color> Data Persistence Manager Not Found! To Save & Load your game, " +
                "load the scene from the MainMenu or any other scene that includes DataPersistenceManager.");
            return;
        }
        SceneManager.LoadScene(SceneToLoad);
    }

    private void UpdateZoneTimer()
    {
        evacuationCountdown--;
        print("aaaaaa");
        UIEnemyTimerUpdater.SetTimeWithPhrase(evacuationCountdown, "teleport after:");
    }

    public void OnEvacuationTriggerExit()
    {
        isEvacuationCountdownStart = false;
    }

    private void StartFirstEvacuationStage()
    {
        timer.AddTimerWithCountDown(EndEvacuationStage, firstStageTimeInSeconds, () => UpdateBaseTimer(), 1);
    }

    private void ActiveEvacuationZones()
    {
        isEvacuationZonesActive = true;
        timer.AddTimerWithCountDown(StartNextEvacuationStage,evacuationZoneActiveTimeInSeconds,()=>UpdateBaseTimer("active zones:"),1);
    }

    private void UpdateBaseTimer(string phrase=null)
    {
        countdown--;
        if (!isEvacuationCountdownStart)
        {            
            UIEnemyTimerUpdater.SetTimeWithPhrase(countdown,phrase);
        }
    }

    private void EndEvacuationStage()
    {
        if (!isEvacuationZonesActive)
        {
            countdown = evacuationZoneActiveTimeInSeconds;
            enemies.Add(Instantiate(enemyTypeDictionary[EnemyType.First], enemySpawnPoints[0]));
            enemies.Add(Instantiate(enemyTypeDictionary[EnemyType.First], enemySpawnPoints[0]));
            ActiveEvacuationZones();
        }
    }

    private void StartNextEvacuationStage()
    {
        isEvacuationZonesActive = false;
        countdown = nextStagesTimeInSeconds;
        timer.AddTimerWithCountDown(EndEvacuationStage, nextStagesTimeInSeconds, () => UpdateBaseTimer(), 1);
    }

    private void Start()
    {
        enemyTypeDictionary = enemyTypeTuple.ToDictionary(t => t.Type, t => t.GameObject);
        timer = G.Timer;
        countdown = firstStageTimeInSeconds;
        evacuationCountdown = evacuationCountdownTimeInSeconds;
        G.PlayerStats.events.OnDamage.AddListener(() => { isDamageRecivedToggle(true); });
        if (gameObject.scene.name == "Winter 1")
            StartFirstEvacuationStage();
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
