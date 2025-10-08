using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace Game.GOAP
{
    [GoapId("PatrollingSensor-9999")]
    public class PatrollingSensor : LocalTargetSensorBase
    {

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var dataPatrolBehaviour = references.GetCachedComponent<DataPatrolBehaviour>();
            var poi = dataPatrolBehaviour.MapAction.GetNearFreePOI(agent.Transform);
        
            if (poi == null)
                return null;
        
            dataPatrolBehaviour.currentPOI = poi;
            
            // If the target is a transform target, set the target to the closest pear
            if (existingTarget is TransformTarget transformTarget)
                return transformTarget.SetTransform(poi.BodyPositionTarget.transform);
        
            return new TransformTarget(poi.BodyPositionTarget.transform);
        }
        // The Created method is called when the sensor is created
        // This can be used to gather references to objects in the scene
        public override void Created() { }

        // This method is equal to the Update method of a local sensor.
        // It can be used to cache data, like gathering a list of all pears in the scene.
        public override void Update() { }
    }
}