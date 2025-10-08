using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace Game.GOAP
{
    [GoapId("IdleTargetSensor-9999")]
    public class IdleTargetSensor : LocalTargetSensorBase
    {
        private static readonly Bounds Bounds = new(Vector3.zero, new Vector3(15, 0, 8));

        public override void Created() { }

        public override void Update() { }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var random = this.GetRandomPosition(agent);

            // If the existing target is a `PositionTarget`, we can reuse it and just update the position.
            if (existingTarget is PositionTarget positionTarget)
            {
                return positionTarget.SetPosition(random);
            }

            return new PositionTarget(random);
        }
        private Vector3 GetRandomPosition(IActionReceiver agent)
        {
            var random = Random.insideUnitCircle * 3f;
            var position = agent.Transform.position + new Vector3(random.x, 0f, random.y);

            // Check if the position is within the bounds of the world.
            if (Bounds.Contains(position))
                return position;

            return Bounds.ClosestPoint(position);
        }

    }
}