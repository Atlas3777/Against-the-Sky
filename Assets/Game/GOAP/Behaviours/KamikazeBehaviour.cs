using cowsins;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using Game.GOAP.Goals;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.GOAP.Behaviours
{
    public class KamikazeBehaviour : MonoBehaviour, IAgentBehaviour
    {
        private AgentBehaviour _agent;
        private GoapActionProvider _provider;
        private GoapBehaviour _goap;
        private EnemyDistancesStats _distancesStats;
        [SerializeField] private EnemyRig enemyRig;
        public bool ShouldAgentInvestigateSound { get; private set; }

        void Awake()
        {
            MyAwake();
        }

        void MyAwake()
        {
            if (!this._goap) _goap = FindFirstObjectByType<GoapBehaviour>();
            if (!this._agent) _agent = this.GetComponent<AgentBehaviour>();
            if (!this._provider) _provider = this.GetComponent<GoapActionProvider>();
            _distancesStats = GetComponent<EnemyDistancesStats>();
            if (!this._provider.AgentTypeBehaviour)
                this._provider.AgentType = this._goap.GetAgentType("KamikazeAgent");
            enemyRig = GetComponent<EnemyRig>();
            enemyRig.Setup(/*G.Player.transform*/);
        }

        void Start()
        {
            this._provider.RequestGoal<PatrollingGoal>();
        }

        void Update()
        {
            var playerPos = G.Player.transform.position;
            // Debug.Log($"dist: {Vector3.Distance(_agent.transform.position, playerPos) < _distancesStats.VisibilityRange}");
            // Debug.Log($"horAngle: {Utils.IsTargetWithinAngle(_agent.transform, playerPos, _distancesStats.HorizontalViewAngle)}");
            // Debug.Log($"vertAngle: {Utils.IsTargetWithinAngle(_agent.transform, playerPos, _distancesStats.VerticalViewAngle)}");
            // Debug.Log($"clearView: {Utils.HasClearView(_agent.Transform, playerPos)}");
            if (Vector3.Distance(_agent.transform.position, playerPos)
                < _distancesStats.VisibilityRange
                && Utils.IsTargetWithinAngle(_agent.transform, playerPos, _distancesStats.HorizontalViewAngle)
                && Utils.IsTargetWithinAngle(_agent.transform, playerPos, _distancesStats.VerticalViewAngle)
                && Utils.HasClearView(_agent.Transform, playerPos))
            {
                enemyRig.UpdateRigWeights(true);
                // Debug.Log("shooting goal requested");
                this._provider.RequestGoal<ShootingGoal>();
                ShouldAgentInvestigateSound = false;
            }
            else if (ShouldAgentInvestigateSound)
            {
                this._provider.RequestGoal<InvestigateSoundGoal>();
            }
            else
            {
                enemyRig.UpdateRigWeights(false);
                this._provider.RequestGoal<PatrollingGoal>();
            }
        }

        public void SwitchAgentSoundInvestigation(bool state)
        {
            ShouldAgentInvestigateSound = state;
        }
    }
}
