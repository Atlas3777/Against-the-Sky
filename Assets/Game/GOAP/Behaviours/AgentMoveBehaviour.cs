using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace Game.GOAP
{
    public class AgentMoveBehaviour : MonoBehaviour
    {
        private AgentBehaviour agent;
        private ITarget currentTarget;
        private NavMeshAgent navMeshAgent;

        private void Awake()
        {
            Debug.Log("agent invoked");
            this.agent = this.GetComponent<AgentBehaviour>();
            this.navMeshAgent = this.GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            Debug.Log("agent enabled");
            this.agent.Events.OnTargetInRange += this.OnTargetInRange;
            this.agent.Events.OnTargetChanged += this.OnTargetChanged;
            this.agent.Events.OnTargetNotInRange += this.TargetNotInRange;
            this.agent.Events.OnTargetLost += this.TargetLost;
        }

        private void OnDisable()
        {
            this.agent.Events.OnTargetInRange -= this.OnTargetInRange;
            this.agent.Events.OnTargetChanged -= this.OnTargetChanged;
            this.agent.Events.OnTargetNotInRange -= this.TargetNotInRange;
            this.agent.Events.OnTargetLost -= this.TargetLost;
        }

        private void TargetLost()
        {
            this.currentTarget = null;
            this.navMeshAgent.ResetPath(); // Останавливаем движение
        }

        private void OnTargetInRange(ITarget target)
        {
            // Цель в зоне действия — останавливаем движение
            this.navMeshAgent.ResetPath();
        }

        private void OnTargetChanged(ITarget target, bool inRange)
        {
            this.currentTarget = target;

            if (target != null && !inRange)
            {
                SetDestination(target.Position);
            }
            else
            {
                this.navMeshAgent.ResetPath();
            }
        }

        private void TargetNotInRange(ITarget target)
        {
            if (target != null)
            {
                SetDestination(target.Position);
            }
        }

        private void SetDestination(Vector3 position)
        {
            Debug.LogWarning("setting destination");
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 10.0f, NavMesh.AllAreas))
            {
                this.navMeshAgent.SetDestination(hit.position);
            }
            else
            {
                Debug.LogWarning("NavMesh не найден для позиции: " + position);
            }
        }

        private void OnDrawGizmos()
        {
            if (this.currentTarget == null)
                return;

            Gizmos.DrawLine(this.transform.position, this.currentTarget.Position);
        }
    }
}