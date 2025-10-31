using UnityEngine;

public class EnemyDistancesStats : MonoBehaviour
{
    [SerializeField] private float attackRange = 3f;
    public float AttackRange => attackRange;
    [SerializeField] private float visibilityRange = 10f;
    public float VisibilityRange => visibilityRange;
    [SerializeField] private float viewAngle = 160f;
    public float ViewAngle => viewAngle;
}
