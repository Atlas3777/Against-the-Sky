using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace Game.GOAP
{
    public class BrainBehaviour : MonoBehaviour
    {
        private AgentBehaviour agent;
        private GoapActionProvider provider;
        private GoapBehaviour goap;
        
        private void Awake()
        {
            myAwake();
        }

        public void myAwake()
        {
            if(this.goap ==null) this.goap = FindObjectOfType<GoapBehaviour>();
            
            if(this.agent is null) agent = this.GetComponent<AgentBehaviour>();
            if(this.provider is null) provider = this.GetComponent<GoapActionProvider>();
            
            // This only applies sto the code demo
            if (this.provider.AgentTypeBehaviour == null)
                this.provider.AgentType = this.goap.GetAgentType("PatrolAgent");
        }

        private void Start()
        {
            this.provider.RequestGoal<PatrollingGoal>();
        }
    }
}