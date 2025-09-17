using UnityEngine;

public class Jump
{
    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;
    public float JumpTimeout = 0.50f;
    public float FallTimeout = 0.15f;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers = LayerMask.GetMask("Default");

    private BodyInputs _input;
    private CharacterController _characterController;
    private PlayerAnimation _animationController;
    private GravityController _gravityController;
    private bool _grounded = false;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private Transform _transform;

    public void Setup(BodyInputs input, PlayerAnimation animationController, Transform transform)
    {
        _input = input;
        Setup(animationController, transform);
    }

    public void Setup(CharacterController characterController, PlayerAnimation animationController, Transform transform)
    {
        _characterController = characterController;
        Setup(animationController, transform);
    }

    public void Setup(PlayerAnimation animationController, Transform transform)
    {
        _animationController = animationController;
        _gravityController = new GravityController(Gravity, 53.0f);
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
        _transform = transform;
    }

    public void UpdateJump()
    {
        GroundedCheck();
        HandleJump();
        UpdateGravity();
        if (_characterController is not null)
        {
            var verticalMove = Vector3.up * _gravityController.GetVerticalVelocity() * Time.deltaTime;
            _characterController.Move(verticalMove);
        }
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(_transform.position.x, _transform.position.y - GroundedOffset,
            _transform.position.z);
        _grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        _animationController?.SetGrounded(_grounded);
    }

    private void HandleJump()
    {
        if (_grounded)
        {
            _fallTimeoutDelta = FallTimeout;
            _animationController?.SetJumpState(false, false);

            if (_input is not null && _input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // Вычисляем вертикальную скорость для прыжка
                float jumpVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                _gravityController.SetVerticalVelocity(jumpVelocity);
                _animationController?.SetJumpState(true, false);
                if (_characterController is not null)
                {
                    Debug.Log("enemy should fall");
                }
            }

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;
            if (_fallTimeoutDelta >= 0.0f)
                _fallTimeoutDelta -= Time.deltaTime;
            else
                _animationController?.SetJumpState(false, true);

            if (_input is not null) _input.jump = false;
        }
        
    }

    private void UpdateGravity()
    {
        _gravityController.UpdateGravity(_grounded);            
    }

    public float GetVerticalVelocity() => _gravityController.GetVerticalVelocity();

    // public void OnDrawGizmosSelected()
    // {
    //     Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
    //     Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

    //     Color gizmoColor = _grounded ? transparentGreen : transparentRed;
    //     Gizmos.color = gizmoColor;

    //     Gizmos.DrawSphere(
    //         new Vector3(_transform.position.x, _transform.position.y - GroundedOffset, _transform.position.z),
    //         GroundedRadius);
    // }
}