using UnityEngine;

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
        if (Physics.Raycast(observer.position, direction, out RaycastHit hit, distance))
        {
            if (!hit.collider.CompareTag("Player"))
                return false;
        }
        return true;
    }
}
