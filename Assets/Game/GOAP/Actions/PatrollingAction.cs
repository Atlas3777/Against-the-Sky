using System;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.GOAP
{
    [GoapId("Patrolling-1cb119ea-f609-41f0-bd31-7b2f254e5b42")]
    public class PatrollingAction : GoapActionBase<PatrollingAction.Data>
    {
        private float _rotationSpeed = 360f;
        private bool _isRotationFinished;
        // This method is called when the action is created
        // This method is optional and can be removed
        public override void Created()
        {
        }

        // This method is called every frame before the action is performed
        // If this method returns false, the action will be stopped
        // This method is optional and can be removed
        public override bool IsValid(IActionReceiver agent, Data data)
        {
            var behaviour = (agent as MonoBehaviour)?.GetComponent<IAgentBehaviour>();
            if (behaviour is not null && behaviour.ShouldAgentInvestigateSound)
                return false;
            return true;
        }

        // This method is called when the action is started
        // This method is optional and can be removed
        public override void Start(IMonoAgent agent, Data data)
        {
            var waypoint = data.DataPatrolBehaviour.CurrWaypoint;
            if (!waypoint)
            {
                Debug.LogWarning("Patrolling action could not start with waypoint " + waypoint);
                return;
            }

            _isRotationFinished = false;
            data.Timer = waypoint.NeedsStop ? waypoint.WaitTime : 0;
        }

        // This method is called once before the action is performed
        // This method is optional and can be removed
        public override void BeforePerform(IMonoAgent agent, Data data)
        {
        }

        // This method is called every frame while the action is running
        // This method is required
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (!data.DataPatrolBehaviour.CurrWaypoint.NeedsStop || data.Timer <= 0f)
                return ActionRunState.Completed;
            
            var waypoint = data.DataPatrolBehaviour.CurrWaypoint;
            if (!_isRotationFinished)
            {
                _isRotationFinished = RotateAgent(agent.transform, waypoint.transform.forward, _rotationSpeed, context.DeltaTime);
                return ActionRunState.Continue;
            }
            data.Timer -= context.DeltaTime;

            return ActionRunState.Continue;
        }

        // This method is called when the action is completed
        // This method is optional and can be removed
        public override void Complete(IMonoAgent agent, Data data)
        {
        }

        // This method is called when the action is stopped
        // This method is optional and can be removed
        public override void Stop(IMonoAgent agent, Data data)
        {
        }

        // This method is called when the action is completed or stopped
        // This method is optional and can be removed
        public override void End(IMonoAgent agent, Data data)
        {
        }
        
        // rotationSpeed принимается в аргументах временно, потом можно будет приписать врагам уникальные скорости
        private bool RotateAgent(Transform agentTransform, Vector3 rotation, float rotationSpeed, float deltaTime)
        {
            rotation.y = 0f;
            if (rotation.sqrMagnitude < 0.001f)
                return true; // нет направления — считаем завершённым

            Quaternion targetRotation = Quaternion.LookRotation(rotation);
            agentTransform.rotation = Quaternion.RotateTowards(agentTransform.rotation, targetRotation, rotationSpeed * deltaTime);
            
            return Quaternion.Angle(agentTransform.rotation, targetRotation) < 0.5f;
        }

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float Timer { get; set; }

            
            [GetComponent]
            public DataPatrolBehaviour DataPatrolBehaviour { get; set; }
        }
    }
}