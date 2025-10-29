using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using Game.GOAP.Sensors;

namespace Game.GOAP.Capabilities
{
    public class PatrollingCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("PatrollingCapability");

            builder.AddGoal<PatrollingGoal>()
                .AddCondition<CountVisitedPoints>(Comparison.GreaterThanOrEqual, 1)
                .SetBaseCost(2);

            builder.AddAction<PatrollingAction>()
                .AddEffect<CountVisitedPoints>(EffectType.Increase)
                .SetTarget<PatrollingPoint>();

            builder.AddTargetSensor<PatrollingSensor>()
                .SetTarget<PatrollingPoint>();

            return builder.Build();
        }
    }
}