using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using Game.GOAP.Capabilities;

namespace Game.GOAP.AgentTypes
{
    public class KamikazeTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var builder = new AgentTypeBuilder("KamikazeAgent");
            builder.AddCapability<PatrollingCapabilityFactory>();
            builder.AddCapability<ChaseCapabilityFactory>();
            builder.AddCapability<ShootingCapabilityFactory>();
            builder.AddCapability<InvestigateSoundCapability>();
            return builder.Build();
        }
    }
}
