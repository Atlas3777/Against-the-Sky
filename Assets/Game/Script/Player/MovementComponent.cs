using UnityEngine;

public class MovementComponent : MonoBehaviour
{
    [Header("Movement Settings")] public float MoveSpeed = 2.0f;
    public float SprintSpeed = 5.335f;
    public float RotationSmoothTime = 0.12f;
    public float SpeedChangeRate = 10.0f;
    public float AimRotationThreshold = 47.0f;

    private CharacterController _controller;
    private BodyInputs _input;
    private GameObject _mainCamera;
    private PlayerAnimation _animationController;

    private PlayerAim _aimController;
    private PlayerJump _jumpController;

    private float _speed;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;

    public void Setup(CharacterController controller, BodyInputs input,
        GameObject mainCamera, PlayerAnimation animationController)
    {
        _controller = controller;
        _input = input;
        _mainCamera = mainCamera;
        _animationController = animationController;
    }

    public void SetDependencies(PlayerJump jumpController)
    {
        _jumpController = jumpController;
    }

    public void UpdateMovement()
    {
        Move();
    }

    private void Move()
    {
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
        if (_input.move == Vector2.zero)
            targetSpeed = 0f;

        _speed = Mathf.Lerp(_speed, targetSpeed, Time.deltaTime * SpeedChangeRate);

        Vector3 forward = _mainCamera.transform.forward;
        Vector3 right = _mainCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * _input.move.y + right * _input.move.x;
        Vector3 velocity = moveDirection.normalized * _speed;
        velocity.y = _jumpController?.GetVerticalVelocity() ?? 0f;

        // Локальное направление движения для Blend Tree
        Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);
        float animVertical = localMoveDirection.z;
        float animHorizontal = localMoveDirection.x;

        // Масштабируем параметры анимации на основе текущей скорости
        float multiplier = _speed / MoveSpeed;

        animVertical *= multiplier;
        animHorizontal *= multiplier;

        bool isIdle = _speed < 0.1f && moveDirection.sqrMagnitude < 0.01f;
        bool isAiming = _input.aim || _input.shooting;

        HandleRotation(moveDirection, isAiming, isIdle);
        UpdateAnimation(animVertical, animHorizontal);

        _controller.Move(velocity * Time.deltaTime);
    }

    private void HandleRotation(Vector3 moveDirection, bool isAiming, bool isIdle)
    {
        if (isAiming)
        {
            HandleAimRotation(moveDirection, isIdle);
        }
        else
        {
            HandleNormalRotation(moveDirection);
        }
    }

    private void HandleAimRotation(Vector3 moveDirection, bool isIdle)
    {
        if (isIdle)
        {
            HandleIdleAimRotation();
        }
        else
        {
            HandleMovingAimRotation(moveDirection);
        }
    }

    private void HandleIdleAimRotation()
    {
        Vector3 cameraForward = _mainCamera.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        float turnAngle = Mathf.DeltaAngle(transform.eulerAngles.y, _mainCamera.transform.eulerAngles.y);

        if (Mathf.Abs(turnAngle) > AimRotationThreshold)
        {
            _animationController?.SetTurning(true, Mathf.Sign(turnAngle));

            _targetRotation = _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
                ref _rotationVelocity, RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
        else
        {
            _animationController?.SetTurning(false, 0);
        }
    }

    private void HandleMovingAimRotation(Vector3 moveDirection)
    {
        float forwardAmount = Vector3.Dot(moveDirection.normalized, transform.forward);

        if (forwardAmount > 0f) // Вперёд
        {
            _targetRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
                ref _rotationVelocity, RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
    }

    private void HandleNormalRotation(Vector3 moveDirection)
    {
        _animationController?.SetTurning(false, 0);

        if (moveDirection != Vector3.zero)
        {
            _targetRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
                ref _rotationVelocity, RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
    }

    private void UpdateAnimation(float animVertical, float animHorizontal)
    {
        _animationController?.UpdateMovementAnimation(animVertical, animHorizontal, _speed);
    }
}