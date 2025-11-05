using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;

namespace Game.GOAP
{
    [GoapId("InvestigatingSoundSensor-cedb2785-5bd7-45e1-92a9-b1a490b2226b")]
    public class InvestigatingSoundSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var dataPatrolBehaviour = references.GetCachedComponent<DataPatrolBehaviour>();
            if (existingTarget is PositionTarget positionTarget)
                return positionTarget.SetPosition(G.Player.transform.position);
        
            return new PositionTarget(G.Player.transform.position);
        }
    }
}