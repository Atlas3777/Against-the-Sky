using UnityEngine;

public class EnemyAim : MonoBehaviour
{
    [Header("Aim Settings")]
    [SerializeField] private float maxGazeDistance = 100f;
    [SerializeField] private Transform aimTarget;
    [SerializeField] private  Transform target;
    public Transform Target { get; private set; }

    void Start()
    {
        target = G.Player.transform;
    }

    private void Update()
    {
        if (!aimTarget || !target) return;

        Vector3 targetChest = target.position + Vector3.up;
        Vector3 direction = (targetChest - transform.position).normalized;
        
        aimTarget.position = targetChest;

        // Ограничение расстояния
        if (Vector3.Distance(transform.position, target.position) > maxGazeDistance)
        {
            aimTarget.position = transform.position + direction * maxGazeDistance;
        }
    }
}