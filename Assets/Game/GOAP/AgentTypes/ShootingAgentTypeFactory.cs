using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace Game.GOAP
{
    public class ShootingAgentTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("ShootingAgent");
            factory.AddCapability<ShootingCapabilityFactory>();

            return factory.Build();
        }
    }
}