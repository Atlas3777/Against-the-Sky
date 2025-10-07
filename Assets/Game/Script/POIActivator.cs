using System;
using UnityEngine;
using UnityEngine.Serialization;

public class POIActivator : MonoBehaviour
{
    [FormerlySerializedAs("pointOfInterest")] public MapAction mapAction;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
            mapAction.StartPointOfInterest();
    }
}
