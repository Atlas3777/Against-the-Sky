using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace Game.GOAP.Sensors
{
    [GoapId("ChaseTargetSensor-9999")]
    public class VisibilityRangeSensor : LocalWorldSensorBase
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
            if (currentDistance > distancesStats.VisibilityRange)
                return 0;
            //Debug.Log("in visibility range");
            if (!Utils.IsTargetWithinAngle(agent.Transform, G.Player.transform.position, distancesStats.ViewAngle))
                return 0;
            if (!Utils.HasClearView(agent.Transform, targetPos))
                return 0;
            return 1;
        }
    }
}