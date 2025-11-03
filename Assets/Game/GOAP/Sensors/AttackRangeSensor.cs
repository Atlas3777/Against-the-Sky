using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace Game.GOAP.Sensors
{
    [GoapId("ChaseTargetSensor-9999")]
    public class AttackRangeSensor : LocalWorldSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            var distancesStats = references.GetCachedComponent<EnemyDistancesStats>();
            var targetPos = G.Player.transform.position; // #MYTODO пока что просто G.Player, а не цель
            var currentDistance = Mathf.Abs(Vector3.Distance(agent.Transform.position, targetPos));
            if (currentDistance > distancesStats.AttackRange)
                return 0;
            if (!Utils.IsTargetWithinAngle(agent.Transform, G.Player.transform.position,
                    distancesStats.HorizontalViewAngle))
                return 0;
            if (!Utils.IsTargetWithinAngle(agent.Transform, G.Player.transform.position,
                    distancesStats.VerticalViewAngle))
                return 0;
            if (!Utils.HasClearView(agent.Transform, targetPos))
                return 0;
            return 1;
            // var distancesStats = references.GetCachedComponent<EnemyDistancesStats>();
            // var targetPos = G.Player.transform.position; // #MYTODO пока что просто G.Player, а не цель
            // var currentDistance = Mathf.Abs(Vector3.Distance(agent.Transform.position, targetPos));
            // return currentDistance <= distancesStats.AttackRange ? 1 : 0;
        }
    }
}