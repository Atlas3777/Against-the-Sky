using System.Collections.Generic;
using cowsins;
using Game.GOAP;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PlayerSoundEmmiter : MonoBehaviour
{
    private class AgentInfo
    {
        public readonly GameObject Agent;
        public readonly IAgentBehaviour AgentBehaviour;

        public AgentInfo(GameObject agent, IAgentBehaviour agentBehaviour)
        {
            Agent = agent;
            AgentBehaviour = agentBehaviour;
        }

        public override bool Equals(object other)
        {
            return other is AgentInfo otherAgent && Agent == otherAgent.Agent;
        }

        public override int GetHashCode()
        {
            return Agent != null ? Agent.GetHashCode() : 0;
        }
    }
    
    [SerializeField] private SphereCollider SphereCollider;
    private HashSet<AgentInfo> _enemiesInRange = new();

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy"))
            return;
        var behaviour = other.GetComponent<IAgentBehaviour>();
        _enemiesInRange.Add(new AgentInfo( other.gameObject, behaviour));
        other.gameObject.GetComponent<EnemyHealth>()?.events.OnDeath.AddListener(() => RemoveEnemy(other.gameObject));
        // Debug.LogWarning("игрок в зоне слышимости");
    }

    void RemoveEnemy(GameObject go)
    {
        _enemiesInRange.RemoveWhere(x => x.Agent == go);
        // Debug.Log($"Враг {go.name} удалён из зоны слышимости");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Enemy"))
            return;
        RemoveEnemy(other.gameObject);
        // Debug.LogWarning("игрок не в зоне слышимости");

    }
    
    void Start()
    {
        G.Player.GetComponent<WeaponController>().events.OnShoot.AddListener(OnPlayerShoot);
    }

    private void OnPlayerShoot()
    {
        if (_enemiesInRange.Count == 0)
            return;
        AgentInfo nearestAgent = null;
        float distance = float.MaxValue;
        foreach (var agent in _enemiesInRange)
        {
            var distToAgent = Vector3.Distance(transform.position, agent.Agent.transform.position);
            if (distToAgent < distance)
            {
                distance = distToAgent;
                nearestAgent = agent;
            }
        }

        if (nearestAgent != null) nearestAgent.AgentBehaviour?.SwitchAgentSoundInvestigation(true);
    }
}
