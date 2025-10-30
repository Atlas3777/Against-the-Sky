using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace Game.GOAP.Sensors
{
    [GoapId("TargetHealthSensor-9999")]
    public class TargetHealthSensor : LocalWorldSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            var currTargetHealth = G.PlayerStats.health; // #MYTODO пока что просто G.Player, а не цель
            //Debug.Log("sense: " + currTargetHealth);
            return new SenseValue((int)currTargetHealth);
        }
    }
}