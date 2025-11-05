using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using Game.GOAP.Actions;
using Game.GOAP.Goals;
using Game.GOAP.Sensors;

namespace Game.GOAP.Capabilities
{
    public class ChaseCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("ChaseCapability");
            
            builder.AddGoal<ChasePlayerGoal>()
                .AddCondition<IsTargetInShootRange>(Comparison.GreaterThanOrEqual, 1)
                //.AddCondition<IsTargetInVisibilityRange>(Comparison.GreaterThanOrEqual, 1)
                .SetBaseCost(2);

            // builder.AddAction<NoticePlayerAction>()
            //     .AddCondition<IsTargetInVisibilityRange>(Comparison.GreaterThanOrEqual, 1)
            //     .AddCondition<IsTargetInShootRange>(Comparison.SmallerThanOrEqual, 0)
            //     .AddEffect<IsTargetInShootRange>(EffectType.Increase)
            //     .SetTarget<PlayerPos>();

            builder.AddAction<ChaseAction>()
                .AddEffect<IsTargetInShootRange>(EffectType.Increase)
                //.AddEffect<IsTargetInVisibilityRange>(EffectType.Increase)
                .AddCondition<IsTargetInVisibilityRange>(Comparison.GreaterThanOrEqual, 1)
                .AddCondition<IsTargetInShootRange>(Comparison.SmallerThan, 1)
                .SetTarget<PlayerPos>()
                .SetStoppingDistance(10f); // #IMPORTANT надо задавать здесь радиус обзора ещё раз ручками из-за особенностей реализации GOAP

            builder.AddWorldSensor<VisibilityRangeSensor>()
                .SetKey<IsTargetInVisibilityRange>();
            builder.AddWorldSensor<AttackRangeSensor>()
                .SetKey<IsTargetInShootRange>();
            builder.AddTargetSensor<ChasingSensor>()
                .SetTarget<PlayerPos>();
            
            return builder.Build();
        }
    }
}