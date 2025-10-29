using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using Game.GOAP.Goals;
using Game.GOAP.Sensors;
using Game.GOAP.WorldKeys;

namespace Game.GOAP.Capabilities
{
    public class ShootingCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("ShootingCapability");

            builder.AddGoal<ShootingGoal>()
                //.AddCondition<IsTargetInShootRange>(Comparison.GreaterThanOrEqual, 1)
                .AddCondition<IsPlayerHealthEqualsZero>(Comparison.SmallerThanOrEqual, 0)
                .SetBaseCost(2);

            builder.AddAction<ShootAction>()
                .AddEffect<IsPlayerHealthEqualsZero>(EffectType.Decrease)
                .AddCondition<IsTargetInShootRange>(Comparison.GreaterThanOrEqual, 1)
                .SetTarget<ShootingPosition>();

            builder.AddTargetSensor<ShootingSensor>()
                .SetTarget<ShootingPosition>();

            return builder.Build();
        }
    }
}