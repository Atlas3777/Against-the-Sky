using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace Game.GOAP
{
    public class ShootingCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("ShootingCapability");

            builder.AddGoal<ShootingGoal>()
                .AddCondition<IsTargetInShootRange>(Comparison.SmallerThanOrEqual, 5)
                .AddCondition<IsPlayerHealthEqualsZero>(Comparison.SmallerThanOrEqual, 0)
                .SetBaseCost(2);

            builder.AddAction<ShootAction>()
                .AddEffect<IsPlayerHealthEqualsZero>(EffectType.Decrease)
                .SetTarget<ShootingPosition>();

            builder.AddTargetSensor<ShootingSensor>()
                .SetTarget<ShootingPosition>();

            return builder.Build();
        }
    }
}