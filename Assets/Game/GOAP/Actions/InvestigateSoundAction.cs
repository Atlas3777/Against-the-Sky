using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using Game.GOAP.Behaviours;
using UnityEngine;

namespace Game.GOAP
{
    [GoapId("InvestigateSound-481b5cd8-4354-4791-9d47-0da3fd9cc28c")]
    public class InvestigateSoundAction : GoapActionBase<InvestigateSoundAction.Data>
    {
        private IAgentBehaviour _agentTypeBehaviour;
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
            _agentTypeBehaviour ??= (agent as MonoBehaviour)?.GetComponent<IAgentBehaviour>();
            Debug.Log("investigating sound...");
            data.Timer = 4f;
        }

        // This method is called once before the action is performed
        // This method is optional and can be removed
        public override void BeforePerform(IMonoAgent agent, Data data)
        {
            // _agentTypeBehaviour.SwitchAgentSoundInvestigation(false);
        }

        // This method is called every frame while the action is running
        // This method is required
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Timer <= 0f)
                return ActionRunState.Completed;
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
            _agentTypeBehaviour.SwitchAgentSoundInvestigation(false);
        }

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float Timer { get; set; }
        }
    }
}