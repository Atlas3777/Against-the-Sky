using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using Game.GOAP.Goals;
using UnityEngine;

namespace Game.GOAP.Behaviours
{
    public class KamikazeBehaviour : MonoBehaviour
    {
        private AgentBehaviour _agent;
        private GoapActionProvider _provider;
        private GoapBehaviour _goap;

        void Awake()
        {
            MyAwake();
        }

        void MyAwake()
        {
            if (!this._goap) _goap = FindFirstObjectByType<GoapBehaviour>();
            if (!this._agent) _agent = this.GetComponent<AgentBehaviour>();
            if (!this._provider) _provider = this.GetComponent<GoapActionProvider>();
            if (!this._provider.AgentTypeBehaviour)
                this._provider.AgentType = this._goap.GetAgentType("KamikazeAgent");
        }

        void Start()
        {
            Debug.Log("KamikazeBehaviour Start");
            //this._provider.RequestGoal<PatrollingGoal>();
            this._provider.RequestGoal<ChasePlayerGoal>();
            //this._provider.RequestGoal<ShootingGoal>();
        }
    }
}
