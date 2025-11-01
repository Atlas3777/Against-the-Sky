using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using Game.GOAP.Capabilities;

namespace Game.GOAP.AgentTypes
{
    public class ShootingAgentTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("ShootingAgent");
            factory.AddCapability<ShootingCapabilityFactory>();
            factory.AddCapability<AFKCapabilityFactory>();

            return factory.Build();
        }
    }
}