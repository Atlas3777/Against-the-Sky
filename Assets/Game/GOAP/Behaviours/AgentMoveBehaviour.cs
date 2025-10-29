using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace Game.GOAP.Behaviours
{
    public class AgentMoveBehaviour : MonoBehaviour
    {
        private AgentBehaviour _agent;
        private ITarget _currentTarget;
        private NavMeshAgent _navMeshAgent;

        private void Awake()
        {
            Debug.Log("agent invoked");
            this._agent = this.GetComponent<AgentBehaviour>();
            this._navMeshAgent = this.GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            Debug.Log("agent enabled");
            this._agent.Events.OnTargetInRange += this.OnTargetInRange;
            this._agent.Events.OnTargetChanged += this.OnTargetChanged;
            this._agent.Events.OnTargetNotInRange += this.TargetNotInRange;
            this._agent.Events.OnTargetLost += this.TargetLost;
        }

        private void OnDisable()
        {
            this._agent.Events.OnTargetInRange -= this.OnTargetInRange;
            this._agent.Events.OnTargetChanged -= this.OnTargetChanged;
            this._agent.Events.OnTargetNotInRange -= this.TargetNotInRange;
            this._agent.Events.OnTargetLost -= this.TargetLost;
        }

        private void TargetLost()
        {
            this._currentTarget = null;
            this._navMeshAgent.ResetPath(); // Останавливаем движение
        }

        private void OnTargetInRange(ITarget target)
        {
            // Цель в зоне действия — останавливаем движение
            this._navMeshAgent.ResetPath();
        }

        private void OnTargetChanged(ITarget target, bool inRange)
        {
            this._currentTarget = target;

            if (target != null && !inRange)
            {
                SetDestination(target.Position);
            }
            else
            {
                this._navMeshAgent.ResetPath();
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
                this._navMeshAgent.SetDestination(hit.position);
            }
            else
            {
                Debug.LogWarning("NavMesh не найден для позиции: " + position);
            }
        }

        private void OnDrawGizmos()
        {
            if (this._currentTarget == null)
                return;

            Gizmos.DrawLine(this.transform.position, this._currentTarget.Position);
        }
    }
}