using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using Game.GOAP.Sensors;

namespace Game.GOAP.Capabilities
{
    public class InvestigateSoundCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("InvestigateSoundCapability");
            
            builder.AddGoal<InvestigateSoundGoal>()
                .AddCondition<IsSoundInvectigated>(Comparison.GreaterThanOrEqual, 1);
            
            builder.AddAction<InvestigateSoundAction>()
                .AddEffect<IsSoundInvectigated>(EffectType.Increase)
                .SetTarget<NoisePoint>();
            
            builder.AddTargetSensor<InvestigatingSoundSensor>()
                .SetTarget<NoisePoint>();
            
            return builder.Build();
        }
    }
}
