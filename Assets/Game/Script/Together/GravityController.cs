using UnityEngine;

public class GravityController
{
    private float _verticalVelocity;
    private float _gravity;
    private float _terminalVelocity;
    private bool _grounded;

    public GravityController(float gravity, float terminalVelocity)
    {
        _gravity = gravity;
        _terminalVelocity = terminalVelocity;
        _verticalVelocity = 0f;
        _grounded = true;
    }

    public void UpdateGravity(bool grounded)
    {
        _grounded = grounded;

        // Применяем гравитацию
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }

        // Если на земле и скорость направлена вниз, сбрасываем до небольшого значения
        if (_grounded && _verticalVelocity < 0.0f)
        {
            _verticalVelocity = -2f;
        }
    }

    public float GetVerticalVelocity() => _verticalVelocity;
    public void SetVerticalVelocity(float velocity) => _verticalVelocity = velocity;
    public bool IsFalling() => _verticalVelocity < 0f && !_grounded;
}