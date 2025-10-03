using System;
using UnityEngine;

public class POIActivator : MonoBehaviour
{
    public PointOfInterest pointOfInterest;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
            pointOfInterest.StartPointOfInterest();
    }
}
