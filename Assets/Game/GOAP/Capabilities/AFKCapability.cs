using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using Game.GOAP.Goals;
using Game.GOAP.Sensors;
using Game.GOAP.WorldKeys;

namespace Game.GOAP.Capabilities
{
    public class AFKCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("AFKCapability");

            builder.AddGoal<AFKGoal>()
                .AddCondition<AFKKey>(Comparison.GreaterThan, 1);

            builder.AddAction<AFKAction>()
                .AddEffect<AFKKey>(EffectType.Increase);

            return builder.Build();
        }
    }
}