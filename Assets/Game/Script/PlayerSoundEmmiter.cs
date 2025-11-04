using System.Linq;
using cowsins;
using UnityEngine;

public class PlayerSoundEmitter : MonoBehaviour
{
    [SerializeField] private GameObject SoundSpheres;
    private PlayerSoundSphere _shootSphere;
    private PlayerSoundSphere _walkSphere;
    private PlayerSoundSphere _crouchSphere;
    private PlayerMovement _movement;

    private float _lastWalkEmitTime;
    [SerializeField] private float stepSoundCooldown = 0.5f;

    private void Start()
    {
        var spheres = SoundSpheres.GetComponentsInChildren<PlayerSoundSphere>();
        _shootSphere = spheres.FirstOrDefault(s => s.SoundType == SoundType.Shooting);
        _walkSphere = spheres.FirstOrDefault(s => s.SoundType == SoundType.Walking);
        _crouchSphere = spheres.FirstOrDefault(s => s.SoundType == SoundType.Crouching);
        var weapon = G.Player.GetComponent<WeaponController>();
        weapon.events.OnShoot.AddListener(() => _shootSphere.NotifyEnemies());

        _movement = G.Player.GetComponent<PlayerMovement>();
        _movement.events.OnMove.AddListener(OnMove);
    }

    private void OnMove()
    {
        if (Time.time - _lastWalkEmitTime < stepSoundCooldown)
            return;
        
        _lastWalkEmitTime = Time.time;

        if (_movement.isCrouching)
            _crouchSphere?.NotifyEnemies();
        else
            _walkSphere?.NotifyEnemies();
    }
}