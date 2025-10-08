using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace Game.GOAP
{
    [GoapId("PatrollingSensor-9999")]
    public class PatrollingSensor : MultiSensorBase
    {
        // A cache of all the pears in the world
        private ActionPOI[] _pois;

        // You must use the constructor to register all the sensors
        // This can also be called outside of the gameplay loop to validate the configuration
        public PatrollingSensor()
        {
            this.AddLocalTargetSensor<PatrollingPoint>((agent, references, target) =>
            {
                var data = references.GetCachedComponent<TestGOAPEnemy>();
                var point = data.MapAction.GetNearFreePOI(agent.Transform).BodyPositionTarget;
        
                if (point == null)
                    return null;
        
                // If the target is a transform target, set the target to the closest pear
                if (target is TransformTarget transformTarget)
                    return transformTarget.SetTransform(point.transform);
        
                return new TransformTarget(point.transform);
            });
        }

        // The Created method is called when the sensor is created
        // This can be used to gather references to objects in the scene
        public override void Created() { }

        // This method is equal to the Update method of a local sensor.
        // It can be used to cache data, like gathering a list of all pears in the scene.
        public override void Update()
        {
            this._pois = Object.FindObjectsOfType<ActionPOI>();
        }
    }
}