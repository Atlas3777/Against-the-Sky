using cowsins;
using cowsins.SaveLoad;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EvacuationZoneTrigger : Trigger
{
    private EvacuationManager evacuationManager;

    private void Start()
    {
        evacuationManager = G.EvacuationManager;
    }

    public override void TriggerEnter(Collider other)
    {
        evacuationManager.OnEvacuationTriggerEnter();
    }

    public override void TriggerExit(Collider other)
    {
        evacuationManager.OnEvacuationTriggerExit();
    }
}
