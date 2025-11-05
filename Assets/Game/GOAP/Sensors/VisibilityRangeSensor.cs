using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace Game.GOAP.Sensors
{
    [GoapId("ChaseTargetSensor-9999")]
    public class VisibilityRangeSensor : LocalWorldSensorBase
    {
        private float visibleTime = 0f;
        private const float requiredVisibleTime = 3f;

        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            var distancesStats = references.GetCachedComponent<EnemyStats>();
            var targetPos = G.Player.transform.position; // #MYTODO пока что просто G.Player, а не цель
            var currentDistance = Mathf.Abs(Vector3.Distance(agent.Transform.position, targetPos));

            var isInRange = currentDistance <= distancesStats.VisibilityRange;
            var inHorizontalAngle = Utils.IsTargetWithinAngle(agent.Transform, targetPos, distancesStats.HorizontalViewAngle);
            var inVerticalAngle = Utils.IsTargetWithinAngle(agent.Transform, targetPos, distancesStats.VerticalViewAngle);
            var hasClearView = Utils.HasClearView(agent.Transform, targetPos);

            if (isInRange && inHorizontalAngle && inVerticalAngle && hasClearView)
            {
                return 1;
                // visibleTime += Time.deltaTime;
                // if (visibleTime >= requiredVisibleTime)
                // {
                //     return 1;
                // }
            }
            // else
            // {
            //     visibleTime = 0f;
            // }

            return 0;
        }
    }
}