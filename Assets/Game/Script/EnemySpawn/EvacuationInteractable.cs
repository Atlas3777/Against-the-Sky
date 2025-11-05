using cowsins;
using UnityEngine;

public class EvacuationInteractable : Interactable
{
    private EvacuationManager2 manager;

    private void Start()
    {
        manager = transform.GetComponent<EvacuationManager2>();
    }
    public override void Interact(Transform player) 
    {
        manager.StartSpawnHelicopterTimer();

        interactableEvents.OnInteract?.Invoke();
    }
}
