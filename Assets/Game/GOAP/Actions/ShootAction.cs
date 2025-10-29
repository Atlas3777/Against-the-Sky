using cowsins;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using Game.Script;
using Unity.Mathematics;
using UnityEngine;

namespace Game.GOAP
{
    [GoapId("Shoot-7f680d5f-9a35-4b65-95bb-cdbeae65c6cb")]
    public class ShootAction : GoapActionBase<ShootAction.Data>
    {

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
            RotateToPlayer(agent, data);
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
            var dist = Vector3.Distance(agent.transform.position, data.Target.Position);
            if (dist > data.DistanceStats.AttackRange)
                return ActionRunState.Stop;
            Debug.Log(dist);
            RotateToPlayer(agent, data);
            data.Weapon.Fire();
            // StartCoroutine(Fire());
            if (G.PlayerStats.health <= 0)
                return ActionRunState.Completed;
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

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            public ITarget Target { get; set; }

            [GetComponentInChildren]
            public WeaponCont Weapon { get; set; }
            [GetComponent]
            public EnemyDistancesStats DistanceStats { get; set; }
        }

        private void RotateToPlayer(IMonoAgent agent, Data data)
        {
            if (G.Player.transform is null)
                return;
            var dir = (G.Player.transform.position - agent.transform.position).normalized;
            dir.y = 0;
            // var currLookDir = agent.transform.forward;
            // agent.transform.rotation = Quaternion.LookRotation(new Vector3(currLookDir.x, dir.y, currLookDir.z));
            agent.transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}