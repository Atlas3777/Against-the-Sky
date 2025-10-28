using System.Collections.Generic;
using UnityEngine;

public class GlobalActionManager
{
    public HashSet<MapAction> mapActions;

    public void RegisterMapAction(MapAction mapAction)
    {
        if(mapAction == null)
            return;
        if (!mapActions.Add(mapAction))
        {
            Debug.LogError("MapActionManager is already registered");
        }
    }
    
    void Update()
    {
        foreach (var mapAction in mapActions)
        {
            if (!mapAction.IsRunning)
            {
                
            }
        }
    }
}