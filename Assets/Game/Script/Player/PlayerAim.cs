using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [Header("Aim Settings")] public float MaxGazeDistance = 100f;
    public LayerMask GazeLayers;
    public float AimRotationThreshold = 50.0f;

    private BodyInputs _input;
    private GameObject _mainCamera;
    private Transform _aimTarget;
    private PlayerRig _rigController;
    private GameObject _weapon;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _rotationSmoothTime = 0.12f;

    public void Setup(BodyInputs input, GameObject mainCamera,
        Transform aimTarget, PlayerRig rigController, GameObject weapon)
    {
        _input = input;
        _mainCamera = mainCamera;
        _aimTarget = aimTarget;
        _rigController = rigController;
        _weapon = weapon;
    }

    public void SetRotationSmoothTime(float smoothTime)
    {
        _rotationSmoothTime = smoothTime;
    }

    public void UpdateAim()
    {
        UpdateGazeTarget();
        HandleAimRotation();
    }

    private void UpdateGazeTarget()
    {
        if (!_aimTarget || !_mainCamera)
            return;

        Vector3 cameraPosition = _mainCamera.transform.position;
        Vector3 cameraForward = _mainCamera.transform.forward;

        Ray ray = new Ray(cameraPosition, cameraForward);
        RaycastHit hit;

        Vector3 targetPosition;

        if (Physics.Raycast(ray, out hit, MaxGazeDistance, GazeLayers))
        {
            targetPosition = hit.point;
        }
        else
        {
            targetPosition = cameraPosition + cameraForward * MaxGazeDistance;
        }

        _aimTarget.position = targetPosition;
    }

    private void HandleAimRotation()
    {
        if (_input.aim || _input.shooting)
        {
            Vector3 cameraForward = _mainCamera.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            float angleToCamera = Vector3.Angle(transform.forward, cameraForward);
            if (angleToCamera > AimRotationThreshold)
            {
                _targetRotation = Mathf.Atan2(cameraForward.x, cameraForward.z) * Mathf.Rad2Deg;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
                    ref _rotationVelocity, _rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
        }
    }
}