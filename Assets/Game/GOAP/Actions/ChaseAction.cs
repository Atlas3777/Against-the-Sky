using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace Game.GOAP.Actions
{
    [GoapId("Chase-cb78b5b0-8637-4750-8f6a-4ff4b90f6961")]
    public class ChaseAction : GoapActionBase<ChaseAction.Data>
    {
        private NavMeshAgent _navMeshAgent;
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
            return true;
        }

        // This method is called when the action is started
        // This method is optional and can be removed
        public override void Start(IMonoAgent agent, Data data)
        {
            if (!_navMeshAgent)
                _navMeshAgent = agent.GetComponent<NavMeshAgent>();
        }

        public override void BeforePerform(IMonoAgent agent, Data data)
        {
        }

        // public override void Start(IMonoAgent agent, Data data)
        // {
        //     Debug.Log("chasing started");
        //     if (!_navMeshAgent)
        //         _navMeshAgent = agent.GetComponent<NavMeshAgent>();
        // }
        //
        // // This method is called once before the action is performed
        // // This method is optional and can be removed
        // public override void BeforePerform(IMonoAgent agent, Data data)
        // {
        //     Debug.Log("before performing");
        // }

        // This method is called every frame while the action is running
        // This method is required
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Target == null)
                return ActionRunState.Stop;

            var stats = data.DistancesStats;
            float distance = Vector3.Distance(agent.transform.position, data.Target.Position);

            // Если игрок вышел за видимость — останавливаем действие
            if (distance > stats.VisibilityRange)
                return ActionRunState.Stop;

            // Если игрок в зоне атаки — завершаем действие
            if (distance < stats.AttackRange)
                return ActionRunState.Completed;

            // Двигаемся к цели
            _navMeshAgent.SetDestination(data.Target.Position);

            return ActionRunState.Continue;
        }
        // public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        // {
        //     _navMeshAgent.SetDestination(data.Target.Position);
        //
        //     var dist = Vector3.Distance(agent.transform.position, data.Target.Position);
        //     Debug.Log(dist);
        //
        //     if (dist < data.DistancesStats.AttackRange)
        //         return ActionRunState.Completed;
        //
        //     if (dist > data.DistancesStats.VisibilityRange)
        //         return ActionRunState.Stop;
        //
        //     return ActionRunState.Continue;
        // }

        // public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        // {
        //     if (Mathf.Abs(Vector3.Distance(data.Target.Position, agent.transform.position)) < data.DistancesStats.AttackRange)
        //         return ActionRunState.Completed;
        //     else if (Mathf.Abs(Vector3.Distance(data.Target.Position, agent.transform.position)) > data.DistancesStats.VisibilityRange)
        //         return ActionRunState.Stop;
        //     
        //     return ActionRunState.Continue;
        // }

        // This method is called when the action is completed
        // This method is optional and can be removed
        public override void Complete(IMonoAgent agent, Data data)
        {
        }

        // This method is called when the action is stopped
        // This method is optional and can be removed
        public override void Stop(IMonoAgent agent, Data data)
        {
            _navMeshAgent.ResetPath();
        }

        // public override void Stop(IMonoAgent agent, Data data)
        // {
        //     Debug.Log("chasing stopped");
        // }

        // This method is called when the action is completed or stopped
        // This method is optional and can be removed
        public override void End(IMonoAgent agent, Data data)
        {
        }

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            
            [GetComponent]
            public EnemyStats DistancesStats { get; set; }
        }
    }
}