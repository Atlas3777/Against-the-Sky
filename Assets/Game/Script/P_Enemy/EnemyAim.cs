using UnityEngine;

public class EnemyAim : MonoBehaviour
{
    [Header("Aim Settings")]
    public float MaxGazeDistance = 100f;
    public Transform aimTarget;
    public Transform target;

    private void Update()
    {
        if (!aimTarget || !target) return;

        Vector3 targetChest = target.position + Vector3.up * 1.2f;
        Vector3 direction = (targetChest - transform.position).normalized;
        
        aimTarget.position = targetChest;

        // Ограничение расстояния
        if (Vector3.Distance(transform.position, target.position) > MaxGazeDistance)
        {
            aimTarget.position = transform.position + direction * MaxGazeDistance;
        }
    }
}