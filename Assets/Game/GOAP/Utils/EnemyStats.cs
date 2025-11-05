using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private float attackRange = 10;
    public float AttackRange => attackRange;
    [SerializeField] private float visibilityRange = 15;
    public float VisibilityRange => visibilityRange;
    [SerializeField] private float horizontalViewAngle = 90;
    public float HorizontalViewAngle => horizontalViewAngle;
    [SerializeField] private float verticalViewAngle = 75;
    public float VerticalViewAngle => verticalViewAngle;
    [SerializeField] private float damagePerBullet = 10;
    public float DamagePerBullet => damagePerBullet;
    [Range(0f, 1f)]
    [SerializeField] private float missChance = 0.5f;
    public float MissChance => missChance;
    [SerializeField] private bool doesDistanceAffectHitting = true;
    public bool DoesDistanceAffectHitting => doesDistanceAffectHitting;
}
