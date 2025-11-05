using System.Collections.Generic;
using cowsins;
using Game.GOAP;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SphereCollider))]
public class PlayerSoundSphere : MonoBehaviour
{

    [Header("Тип звука этой сферы")]
    [SerializeField] private SoundType soundType;
    public SoundType SoundType => soundType;

    private HashSet<IAgentBehaviour> _enemiesInRange = new();
    private SphereCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;
        var behaviour = other.GetComponent<IAgentBehaviour>();
        if (behaviour == null) return;

        _enemiesInRange.Add(behaviour);

        other.GetComponent<EnemyHealth>()?.events.OnDeath.AddListener(() => RemoveEnemy(behaviour));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;
        var behaviour = other.GetComponent<IAgentBehaviour>();
        RemoveEnemy(behaviour);
    }

    private void RemoveEnemy(IAgentBehaviour behaviour)
    {
        _enemiesInRange.Remove(behaviour);
    }

    public void NotifyEnemies()
    {
        if (_enemiesInRange.Count == 0)
            return;

        IAgentBehaviour nearest = null;
        var minDist = float.MaxValue;

        foreach (var enemy in _enemiesInRange)
        {
            var dist = Utils.GetNavMeshDistance(((MonoBehaviour)enemy).transform.position, transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        
        
        nearest?.SwitchAgentSoundInvestigation(true);
        Debug.Log($"[{soundType}] услышал ближайший враг на расстоянии {minDist:F1} м");
    }

    private void OnDrawGizmos()
    {
        if (_collider == null)
            _collider = GetComponent<SphereCollider>();

        Gizmos.color = soundType switch
        {
            SoundType.Shooting => Color.red,
            SoundType.Walking => Color.yellow,
            SoundType.Crouching => Color.cyan,
            _ => Color.white
        };
        Gizmos.DrawWireSphere(transform.position, _collider.radius);
    }
}
