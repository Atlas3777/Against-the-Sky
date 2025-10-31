using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    private List<(Action Event, float Start, float Seconds)> timers = new();
    private List<(Func<bool> deleter, (Action Event, float Start, float Seconds) Timer)> deleters = new();

    public void AddTimer(Action ev, float timeInSeconds, Func<bool> deleteWith = null)
    {
        var t = (ev, Time.time, timeInSeconds);
        timers.Add(t);
        if (!(deleteWith is null))
            deleters.Add((deleteWith,t));
    }

    public void AddTimerWithCountDown(Action finalEvent, float timeInSeconds, Action countdownEvent, float countDownDelay,
        Func<bool> deleteWith=null)
    {
        var isFinalTimerEnds = false;
        Func<bool> stopper = () => isFinalTimerEnds;
        Func<bool> deleteMain = () => 
        {
            if (!(deleteWith is null))
            {
                if (deleteWith())
                {
                    isFinalTimerEnds = true;
                    return true;
                }
            }
            return false;
        };
        RecursionTimer(countdownEvent, countDownDelay, Time.time-countDownDelay, stopper,deleteWith);
        AddTimer(() => { finalEvent(); isFinalTimerEnds = true; }, timeInSeconds,deleteMain);
    }

    private void RecursionTimer(Action ev, float timeInSeconds, float expectedStartTime, Func<bool> stopper,
        Func<bool> deleteWith = null) =>
        AddTimer(() => 
            {
                if (!stopper())
                {
                    ev();
                    RecursionTimer(ev, timeInSeconds, expectedStartTime + timeInSeconds, stopper);
                }
            },
            expectedStartTime + 2 * timeInSeconds - Time.time,
            deleteWith);

    private void Update()
    {
        if (timers.Count > 0)
        {
            List<(Action Event, float Start, float Seconds)> timersToDelete = new();
            foreach (var t in timers.OrderBy(ti=>ti.Start).ToArray())
            {
                if (t.Start+t.Seconds <= Time.time)
                {
                    timersToDelete.Add(t);
                    t.Event.Invoke();
                }
            }
            foreach (var d in deleters)
            {
                if (d.deleter())
                    timersToDelete.Add(d.Timer);
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
