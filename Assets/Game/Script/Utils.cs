using UnityEngine;
using UnityEngine.AI;

public static class Utils
{
    public static bool IsTargetWithinAngle(Transform objTransform, Vector3 targetPosition, float angle)
    {
        var toTarget = (targetPosition - objTransform.position).normalized;
        var angleToTarget = Vector3.Angle(objTransform.forward, toTarget);
        //Debug.Log($"angleToTarget: {angleToTarget}, angle: {angle}, tvar: {angleToTarget <= (angle / 2f)}");
        return angleToTarget <= (angle / 2f);
    }
    
    public static bool HasClearView(Transform observer, Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - observer.position).normalized;
        float distance = Vector3.Distance(observer.position, targetPosition);
        if (Physics.Raycast(observer.position, direction, out RaycastHit hit, distance, ~0 ,QueryTriggerInteraction.Ignore))
        {
            // Debug.Log("ray");
            if (!hit.collider.CompareTag("Player"))
            {
                //Debug.Log("not to player");
                return false;
            }
        }
        return true;
    }
    
    public static float GetNavMeshDistance(Vector3 start, Vector3 end)
    {
        var path = new NavMeshPath();
        if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
        {
            var distance = 0f;
            for (int i = 1; i < path.corners.Length; i++)
                distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            return distance;
        }
        return Mathf.Infinity;
    }
}
