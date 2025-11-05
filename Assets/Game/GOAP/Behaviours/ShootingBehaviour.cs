using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using Game.GOAP;
using Game.GOAP.Goals;
using UnityEngine;
using UnityEngine.AI;

public class ShootingBehaviour : MonoBehaviour
{
    private AgentBehaviour _agent;
    private GoapActionProvider _provider;
    private GoapBehaviour _goap;
    [SerializeField] private EnemyRig enemyRig;
    private EnemyStats _distancesStats;

    void Awake()
    {
        if(this._goap ==null) this._goap = FindFirstObjectByType<GoapBehaviour>();
        
        if(this._agent is null) _agent = this.GetComponent<AgentBehaviour>();
        if (this._provider is null) _provider = this.GetComponent<GoapActionProvider>();
        _distancesStats = GetComponent<EnemyStats>();


        if (this._provider.AgentTypeBehaviour == null)
            this._provider.AgentType = this._goap.GetAgentType("ShootingAgent");
        enemyRig = GetComponent<EnemyRig>();
        enemyRig.Setup(/*G.Player.transform*/);
        
    }
    
    void Start()
    {
        this._provider.RequestGoal<AFKGoal>();
    }

    // Update is called once per frame
    void Update()
    {
        var playerPos = G.Player.transform.position;
        
        // Debug.Log($"time: {Time.time}, distance {Vector3.Distance(_agent.transform.position, playerPos) < _distancesStats.VisibilityRange}");
        // Debug.Log($"time: {Time.time}, IsTargetWithinAngle {Utils.IsTargetWithinAngle(_agent.transform, playerPos, _distancesStats.ViewAngle)}");
        // Debug.Log($"time: {Time.time}, HasClearView {Utils.HasClearView(_agent.Transform, playerPos)}");
        if (Vector3.Distance(_agent.transform.position, playerPos)
            < _distancesStats.VisibilityRange
            && Utils.IsTargetWithinAngle(_agent.transform, playerPos, _distancesStats.HorizontalViewAngle)
            && Utils.IsTargetWithinAngle(_agent.transform, playerPos, _distancesStats.VerticalViewAngle)
            && Utils.HasClearView(_agent.Transform, playerPos))
        {
            // Debug.Log("RfrFRFRFFR");
            enemyRig.UpdateRigWeights(true);
            this._provider.RequestGoal<ShootingGoal>();
        }
        else
        {
            enemyRig.UpdateRigWeights(false);
            this._provider.RequestGoal<AFKGoal>();
        }
    }
}
