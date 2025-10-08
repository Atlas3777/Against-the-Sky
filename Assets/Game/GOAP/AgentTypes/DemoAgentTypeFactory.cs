using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace Game.GOAP
{
    public class DemoAgentTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("ScriptDemoAgent");
            
            factory.AddCapability<PatrollingCapabilityFactory>();

            return factory.Build();
        }
    }
}