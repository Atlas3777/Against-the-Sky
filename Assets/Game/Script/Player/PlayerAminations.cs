using StarterAssets;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator _animator;
    private BodyInputs _input;
    private bool _hasAnimator;

    // Animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    private int _animIDShoot;
    private int _animIDIsTurning;
    private int _animIDTurnDirection;

    public void Setup(Animator animator, BodyInputs input)
    {
        _animator = animator;
        _input = input;
        _hasAnimator = animator != null;
        AssignAnimationIDs();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDShoot = Animator.StringToHash("Shoot");
        _animIDIsTurning = Animator.StringToHash("IsTurning");
        _animIDTurnDirection = Animator.StringToHash("TurnDirection");
    }

    public void UpdateMovementAnimation(float vertical, float horizontal, float speed)
    {
        if (!_hasAnimator) return;

        _animator.SetFloat("Vertical", vertical, 0.1f, Time.deltaTime);
        _animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
        _animator.SetFloat(_animIDSpeed, speed);
            //_animator.SetFloat(_animIDMotionSpeed, speed);
    }

    public void SetGrounded(bool grounded)
    {
        if (!_hasAnimator) return;
        _animator.SetBool(_animIDGrounded, grounded);
    }

    public void SetJumpState(bool jump, bool freeFall)
    {
        if (!_hasAnimator) return;
        _animator.SetBool(_animIDJump, jump);
        _animator.SetBool(_animIDFreeFall, freeFall);
    }

    public void SetShooting(bool shooting)
    {
        if (!_hasAnimator) return;
        _animator.SetBool(_animIDShoot, shooting);
    }

    public void SetTurning(bool isTurning, float turnDirection)
    {
        if (!_hasAnimator) return;
        _animator.SetBool(_animIDIsTurning, isTurning);
        if (isTurning)
            _animator.SetFloat(_animIDTurnDirection, turnDirection);
    }

    public void UpdateShooting()
    {
        SetShooting(_input.shooting);
    }
}