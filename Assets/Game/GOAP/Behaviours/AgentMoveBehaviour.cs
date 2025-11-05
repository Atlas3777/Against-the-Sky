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
        private Animator _animator;

        private void Awake()
        {
            this._agent = this.GetComponent<AgentBehaviour>();
            this._navMeshAgent = this.GetComponent<NavMeshAgent>();
            this._animator = this.GetComponent<Animator>();
        }

        private void OnEnable()
        {
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
        
        public void OnFootstep(AnimationEvent animationEvent)
        {
            // if (animationEvent.animatorClipInfo.weight > 0.5f)
            // {
            //     if (FootstepAudioClips.Length > 0)
            //     {
            //         var index = Random.Range(0, FootstepAudioClips.Length);
            //         AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center),
            //             FootstepAudioVolume);
            //     }
            // }
        }

        void Update()
        {
            if (!_navMeshAgent || !_animator)
                return;
            var velocity = _navMeshAgent.velocity;
            var speed = velocity.magnitude;

            if (speed < 0.05f)
            {
                _animator.SetFloat("Horizontal", 0f, 0.1f, Time.deltaTime);
                _animator.SetFloat("Vertical", 0f, 0.1f, Time.deltaTime);
                return;
            }
            
            var localVel = transform.InverseTransformDirection(velocity.normalized);
            
            _animator.SetFloat("Horizontal", localVel.x, 0.1f, Time.deltaTime);
            _animator.SetFloat("Vertical", localVel.z, 0.1f, Time.deltaTime);
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