using UnityEngine;

public class EnemyDistancesStats : MonoBehaviour
{
    [SerializeField] private float attackRange = 10;
    public float AttackRange => attackRange;
    [SerializeField] private float visibilityRange = 15;
    public float VisibilityRange => visibilityRange;
    [SerializeField] private float horizontalViewAngle = 90;
    public float HorizontalViewAngle => horizontalViewAngle;
    [SerializeField] private float verticalViewAngle = 75;
    public float VerticalViewAngle => verticalViewAngle;
}
