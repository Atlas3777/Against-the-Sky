using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace Game.GOAP.Sensors
{
    [GoapId("ShootingSensor-1")]
    public class ShootingSensor : LocalTargetSensorBase
    {
        private Transform _player;
        public override void Created()
        {
            if (G.Player is not null)
                _player = G.Player.transform;
            else
                Debug.LogWarning("G.Player was not avialable");
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var distanceToTarget = Vector3.Distance(agent.Transform.position, _player.position);
            var isPlayerDead = G.PlayerStats.health <= 0;

            if (!isPlayerDead && distanceToTarget <= 5f)
                return new PositionTarget(_player.position);
            return new PositionTarget(agent.Transform.position);;
        }

        public override void Update()
        { }
    }
}