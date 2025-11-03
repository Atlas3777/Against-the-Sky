using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class EnemyGizmos : MonoBehaviour
{
    [SerializeField] private EnemyDistancesStats distancesStats;
    [Header("Target")]
    public Transform player;

    [Header("View / detection")]
    [Tooltip("Горизонтальный угол обзора (в градусах)")]
    public float horizontalFov;
    [Tooltip("Вертикальный угол обзора (в градусах)")]
    public float verticalFov;
    [Tooltip("Дальность обзора (длина конуса)")]
    public float viewDistance;
    [Tooltip("Радиус мгновенного обнаружения (вблизи)")]
    public float immediateDetectRadius;
    [Tooltip("Если игрок виден за пределами голубой зоны — требуется это время (сек) для замечания")]
    public float timeToNotice = 3f;

    [Header("Connections")]
    public Transform[] connectionPoints;

    [Header("Colors")]
    public Color viewFrustumWire = new Color(0f, 0f, 0f, 0.6f);
    public Color farPlaneFill = new Color(1f, 0.65f, 0f, 0.1f);   // дальняя плоскость (оранжевая)
    public Color nearPlaneFill = new Color(0f, 0.5f, 1f, 0.25f); // ближняя плоскость (голубая)
    public Color connectionColor = Color.red;

    void OnValidate()
    {
        horizontalFov = distancesStats.HorizontalViewAngle;
        verticalFov = distancesStats.VerticalViewAngle;
        viewDistance = distancesStats.VisibilityRange;
        immediateDetectRadius = distancesStats.AttackRange;
    }

    void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        Vector3 fwd = transform.forward;
        Vector3 up = transform.up;
        Vector3 right = transform.right;

        // --- Параметры фрустума ---
        float halfVert = verticalFov * 0.5f * Mathf.Deg2Rad;
        float halfHorz = horizontalFov * 0.5f * Mathf.Deg2Rad;
        float near = immediateDetectRadius;
        float far = viewDistance;

        // Размеры ближней и дальней плоскостей
        float nearHeight = 2f * Mathf.Tan(halfVert) * near;
        float nearWidth = 2f * Mathf.Tan(halfHorz) * near;
        float farHeight = 2f * Mathf.Tan(halfVert) * far;
        float farWidth = 2f * Mathf.Tan(halfHorz) * far;

        // Центры
        Vector3 nearCenter = pos + fwd * near;
        Vector3 farCenter = pos + fwd * far;

        // --- Углы плоскостей ---
        Vector3[] nearCorners = new Vector3[4];
        Vector3[] farCorners = new Vector3[4];

        // Ближняя плоскость
        nearCorners[0] = nearCenter + up * (nearHeight / 2f) - right * (nearWidth / 2f); // TL
        nearCorners[1] = nearCenter + up * (nearHeight / 2f) + right * (nearWidth / 2f); // TR
        nearCorners[2] = nearCenter - up * (nearHeight / 2f) - right * (nearWidth / 2f); // BL
        nearCorners[3] = nearCenter - up * (nearHeight / 2f) + right * (nearWidth / 2f); // BR

        // Дальняя плоскость
        farCorners[0] = farCenter + up * (farHeight / 2f) - right * (farWidth / 2f);
        farCorners[1] = farCenter + up * (farHeight / 2f) + right * (farWidth / 2f);
        farCorners[2] = farCenter - up * (farHeight / 2f) - right * (farWidth / 2f);
        farCorners[3] = farCenter - up * (farHeight / 2f) + right * (farWidth / 2f);

        // --- Закрашивание ближней и дальней плоскостей ---
#if UNITY_EDITOR
        Handles.color = nearPlaneFill;
        Handles.DrawAAConvexPolygon(nearCorners);

        Handles.color = farPlaneFill;
        Handles.DrawAAConvexPolygon(farCorners);
#endif

        // --- Проволочная сетка фрустума ---
        Gizmos.color = viewFrustumWire;

        // ближний контур
        for (int i = 0; i < 4; i++)
            Gizmos.DrawLine(nearCorners[i], nearCorners[(i + 1) % 4]);

        // дальний контур
        for (int i = 0; i < 4; i++)
            Gizmos.DrawLine(farCorners[i], farCorners[(i + 1) % 4]);

        // рёбра
        for (int i = 0; i < 4; i++)
            Gizmos.DrawLine(nearCorners[i], farCorners[i]);

        // лучи от центра к углам
        Gizmos.DrawLine(pos, farCorners[0]);
        Gizmos.DrawLine(pos, farCorners[1]);
        Gizmos.DrawLine(pos, farCorners[2]);
        Gizmos.DrawLine(pos, farCorners[3]);

        // --- Соединения ---
        if (connectionPoints != null && connectionPoints.Length > 0)
        {
            Gizmos.color = connectionColor;
            foreach (var t in connectionPoints)
            {
                if (t == null) continue;
                Gizmos.DrawLine(pos, t.position);
                Gizmos.DrawSphere(t.position, 0.08f);
            }
        }

        // --- Игрок ---
        if (player != null)
        {
            float dist = Vector3.Distance(pos, player.position);
            Gizmos.color = dist <= immediateDetectRadius ? Color.cyan : Color.yellow;
            Gizmos.DrawWireSphere(player.position, 0.15f);
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(pos, player.position);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        string info = $"H: {horizontalFov}° | V: {verticalFov}° | Dist: {viewDistance}m | Near: {immediateDetectRadius}m | Notice: {timeToNotice:F1}s";
        Handles.Label(transform.position + Vector3.up * 1.5f, info);
    }
#endif
}
