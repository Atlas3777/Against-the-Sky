using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PatrolRoute : MonoBehaviour
{
    [SerializeField] private Waypoint[] waypoints;
    public Waypoint[] Waypoints => waypoints;
    [SerializeField] private bool isRouteCycle;
    private void OnValidate()
    {
        waypoints = GetComponentsInChildren<Waypoint>().ToArray();
        if (!isRouteCycle)
            waypoints = waypoints.Concat(waypoints.Reverse().Skip(1).Take(waypoints.Length - 2)).ToArray();
    }
    
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2)
            return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i + 1].transform.position);
        }
    }
}
