using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace Game.GOAP.Behaviours
{
    public class BrainBehaviour : MonoBehaviour
    {
        private AgentBehaviour _agent;
        private GoapActionProvider _provider;
        private GoapBehaviour _goap;
        
        private void Awake()
        {
            MyAwake();
        }

        public void MyAwake()
        {
            if(!this._goap) this._goap = FindFirstObjectByType<GoapBehaviour>();
            
            if(this._agent is null) _agent = this.GetComponent<AgentBehaviour>();
            if(this._provider is null) _provider = this.GetComponent<GoapActionProvider>();
            
            // This only applies sto the code demo
            if (!this._provider.AgentTypeBehaviour)
                this._provider.AgentType = this._goap.GetAgentType("PatrolAgent");
        }

        private void Start()
        {
            this._provider.RequestGoal<PatrollingGoal>();
        }
    }
}