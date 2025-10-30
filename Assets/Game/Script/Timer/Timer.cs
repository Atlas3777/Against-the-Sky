using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    private List<(Action Event, float Start, float Seconds)> timers = new();

    public void AddTimer(Action ev, float timeInSeconds) =>
        timers.Add((ev, Time.time, timeInSeconds));

    public void AddTimerWithCountDown(Action finalEvent, float timeInSeconds, Action countdownEvent, float countDownDelay)
    {
        var isFinalTimerEnds = false;
        Func<bool> stopper = () => isFinalTimerEnds;
        RecursionTimer(countdownEvent, countDownDelay, Time.time-countDownDelay, stopper);
        AddTimer(() => { finalEvent(); isFinalTimerEnds = true; }, timeInSeconds);
    }

    private void RecursionTimer(Action ev, float timeInSeconds, float expectedStartTime, Func<bool> stopper) =>
        AddTimer(() => 
            {
                if (!stopper())
                {
                    ev();
                    RecursionTimer(ev, timeInSeconds, expectedStartTime + timeInSeconds, stopper);
                }
            },
            expectedStartTime + 2 * timeInSeconds - Time.time);

    private (Action Event, float Start, float Seconds) GetLastTimer(Action countdownEvent, float countDownDelay)
    {
        return (() =>
            {
                countdownEvent();
                
            },
            Time.time,
            countDownDelay);
    }

    private void Update()
    {
        if (timers.Count > 0)
        {
            List<(Action Event, float Start, float Seconds)> timersToDelete = new();
            foreach (var t in timers.ToArray())
            {
                if (t.Start+t.Seconds <= Time.time)
                {
                    timersToDelete.Add(t);
                    t.Event.Invoke();
                }
            }
            DeleteTimers(timersToDelete);
        }
    }

    private void DeleteTimers(List<(Action Event, float Start, float Seconds)> timersToDelete)
    {
        foreach (var t in timersToDelete)
            timers.Remove(t);
    }

    private void OnDisable()
    {
        timers.Clear();
    }
}
