using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace Game.GOAP
{
    public class PatrolTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("PatrolAgent");
            
            factory.AddCapability<PatrollingCapabilityFactory>();

            return factory.Build();
        }
    }
}