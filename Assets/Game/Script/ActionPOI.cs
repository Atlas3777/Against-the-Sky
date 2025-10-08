using UnityEngine;

public class ActionPOI : MonoBehaviour
{
    public Transform BodyPositionTarget;
    public float timeCooldown = 2f; // Время блокировки после освобождения

    private float _releaseTime; // Время, когда точка снова станет доступной
    private bool _isOccupied = false;

    public bool IsOccupied => _isOccupied;
    public bool IsAvailable => !_isOccupied && Time.time >= _releaseTime;

    // Занять точку
    public void Occupy()
    {
        if (IsAvailable)
        {
            _isOccupied = true;
        }
        else
        {
            Debug.LogWarning("Trying to occupy an unavailable POI!");
        }
    }

    // Освободить точку (начинается отсчёт cooldown)
    public void Release()
    {
        _isOccupied = false;
        _releaseTime = Time.time + timeCooldown;
    }
}