using UnityEngine;

public class Jump : MonoBehaviour
{
    [Header("Jump Settings")] 
    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;
    public float JumpTimeout = 0.50f;
    public float FallTimeout = 0.15f;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;

    private BodyInputs _input;
    private PlayerAnimation _animationController;
    private GravityController _gravityController;
    private bool _grounded = true;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    public void Setup(BodyInputs input, PlayerAnimation animationController)
    {
        _input = input;
        _animationController = animationController;
        _gravityController = new GravityController(Gravity, 53.0f);
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    public void UpdateJump()
    {
        GroundedCheck();
        HandleJump();
        UpdateGravity();
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
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

            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // Вычисляем вертикальную скорость для прыжка
                float jumpVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                _gravityController.SetVerticalVelocity(jumpVelocity);
                _animationController?.SetJumpState(true, false);
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

            _input.jump = false;
        }
    }

    private void UpdateGravity()
    {
        _gravityController.UpdateGravity(_grounded);
    }

    public float GetVerticalVelocity() => _gravityController.GetVerticalVelocity();

    public void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Color gizmoColor = _grounded ? transparentGreen : transparentRed;
        Gizmos.color = gizmoColor;

        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }
}