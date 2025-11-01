using UnityEngine;
public class Waypoint : MonoBehaviour
{
    [SerializeField] private bool needsStop;
    public bool NeedsStop => needsStop;

    [SerializeField] private float waitTime;
    public float WaitTime => waitTime;

    private void OnDrawGizmos()
    {
        Gizmos.color = needsStop ? Color.green : Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.2f);

        // направление взгляда
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
    }
}