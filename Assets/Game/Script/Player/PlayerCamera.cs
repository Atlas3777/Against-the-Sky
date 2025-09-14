using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Settings")] public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    public float CameraAngleOverride = 0.0f;
    public bool LockCameraPosition = false;

    private BodyInputs _input;
    private GameObject _cinemachineCameraTarget;
    private bool _isMouseInput;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float _threshold = 0.01f;

    public void Setup(BodyInputs input, GameObject cinemachineTarget, bool isMouseInput)
    {
        _input = input;
        _cinemachineCameraTarget = cinemachineTarget;
        _isMouseInput = isMouseInput;

        if (_cinemachineCameraTarget != null)
        {
            _cinemachineTargetYaw = _cinemachineCameraTarget.transform.rotation.eulerAngles.y;
        }
    }

    public void UpdateCamera()
    {
        // Пусто - основная логика в LateUpdate
    }

    public void LateUpdateCamera()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        if (_input == null || _cinemachineCameraTarget == null) return;

        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            float deltaTimeMultiplier = _isMouseInput ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        _cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}