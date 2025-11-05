using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace Game.GOAP.Actions
{
    [GoapId("NoticePlayer-74aa328f-6e79-4b01-a3b5-098e493b6c34")]
    public class NoticePlayerAction : GoapActionBase<NoticePlayerAction.Data>
    {
        private NavMeshAgent _navMeshAgent;
        private Animator _animator;
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
            if (!_animator)
                _animator = agent.GetComponent<Animator>();
            
            _navMeshAgent.isStopped = true;
            _navMeshAgent.ResetPath();
            data.Timer = 0f;
            
            _animator.SetFloat("Horizontal", 0); //MYTODO прям щас проверь
            _animator.SetFloat("Vertical", 0);
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
            data.Timer += Time.deltaTime;
            
            if (data.Timer >= 3f)
                return ActionRunState.Completed;
            return ActionRunState.Continue;
        }

        // This method is called when the action is completed
        // This method is optional and can be removed
        public override void Complete(IMonoAgent agent, Data data)
        {
            _navMeshAgent.isStopped = false;
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

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            public float Timer;
            public ITarget Target { get; set; }
            
            // [GetComponent]
            // public UnityEngine.AI.NavMeshAgent NavMeshAgent { get; set; }
            // [GetComponent]
            // public Animator Animator { get; set; }
        }
    }
}