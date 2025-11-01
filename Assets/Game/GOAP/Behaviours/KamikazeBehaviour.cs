using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using Game.GOAP.Goals;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.GOAP.Behaviours
{
    public class KamikazeBehaviour : MonoBehaviour
    {
        private AgentBehaviour _agent;
        private GoapActionProvider _provider;
        private GoapBehaviour _goap;
        private EnemyDistancesStats _distancesStats;
        [SerializeField] private EnemyRig enemyRig;

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
            Debug.Log("KamikazeBehaviour Start");
            this._provider.RequestGoal<PatrollingGoal>();
            //this._provider.RequestGoal<ChasePlayerGoal>();
        }

        void Update()
        {
            var playerPos = G.Player.transform.position;
            if (Vector3.Distance(_agent.transform.position, playerPos)
                < _distancesStats.VisibilityRange
                && Utils.IsTargetWithinAngle(_agent.transform, playerPos, _distancesStats.ViewAngle)
                && Utils.HasClearView(_agent.Transform, playerPos))
            {
                enemyRig.UpdateRigWeights(true);
                this._provider.RequestGoal<ShootingGoal>();
            }
            else
            {
                enemyRig.UpdateRigWeights(false);
                this._provider.RequestGoal<PatrollingGoal>();
            }
        }
    }
}
