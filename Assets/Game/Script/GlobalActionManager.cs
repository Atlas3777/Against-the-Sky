using UnityEngine;

public class GlobalActionManager
{
    private MapAction[] mapActions;
    
    public void FindAllMapAction()
    {
        mapActions = Object.FindObjectsByType<MapAction>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }
}