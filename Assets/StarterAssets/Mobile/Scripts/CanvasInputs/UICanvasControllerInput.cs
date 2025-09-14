using UnityEngine;
using UnityEngine.Serialization;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {
        [FormerlySerializedAs("starterAssetsInputs")] [Header("Output")]
        public BodyInputs bodyInputs;

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            bodyInputs.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            bodyInputs.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            bodyInputs.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            bodyInputs.SprintInput(virtualSprintState);
        }
        
    }

}
