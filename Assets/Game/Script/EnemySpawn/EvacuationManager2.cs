using UnityEngine;

public class EvacuationManager2 : MonoBehaviour 
{
    [SerializeField] private int helicopterDelayInSeconds;
    [SerializeField] private GameObject helicopter;
    private Timer timer;
    private int countdown;

    private void Start()
    {
        timer = G.Timer;
    }

    public void StartSpawnHelicopterTimer()
    {
        countdown = helicopterDelayInSeconds;
        timer.AddTimerWithCountDown(SpawnHelicopter,helicopterDelayInSeconds,UpdateTimer,1);
    }

    private void UpdateTimer()
    {
        countdown--;
        UIEnemyTimerUpdater.SetTimeWithPhrase(countdown,"helicopter after:");
    }

    private void SpawnHelicopter()
    {
        helicopter.SetActive(true);
        UIEnemyTimerUpdater.StopVisual();
    }
}
