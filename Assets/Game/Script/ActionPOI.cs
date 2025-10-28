using UnityEngine;
using UnityEngine.Serialization;

public class ActionPOI : MonoBehaviour
{
    [SerializeField] private Transform bodyPositionTarget;
    [SerializeField] private float timeCooldown = 2f;

    private float _releaseTime;
    private bool _isOccupied = false;
    
    public Transform BodyPositionTarget;
    public bool IsOccupied => _isOccupied;
    public bool IsAvailable => !_isOccupied && Time.time >= _releaseTime;

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

    public void Release()
    {
        _isOccupied = false;
        _releaseTime = Time.time + timeCooldown;
    }
}