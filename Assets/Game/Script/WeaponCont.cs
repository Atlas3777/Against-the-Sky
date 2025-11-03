using System.Collections;
using Andtech.ProTracer;
using cowsins;
using StarterAssets;
using UnityEngine;

namespace Game.Script
{
    public class WeaponCont : MonoBehaviour
    {
        public float ShotsPerSecond = 7.0f; // Matches rateOfFire from TracerDemo
        public LayerMask hitLayer;
        public GameObject ImpactEffect; // Эффект у ствола
        public GameObject HitEffect; // Эффект попадания
        public Transform ImpactEffectTransform;
        public AudioClip shootSound; // Аудиоклип для звука выстрела
        public AudioClip impactShootSound; // для попадания
        public int damagePerBullet = 10;
        public float timeBetweenShots = 1f;
        private bool _isReload;
        private float _raycastDistance;

        [Header("Tracer Prefabs")]
        [SerializeField]
        [Tooltip("The Bullet prefab to spawn.")]
        // private Bullet bulletPrefab = default;
        // [SerializeField]
        // [Tooltip("The Smoke Trail prefab to spawn.")]
        // private SmokeTrail smokeTrailPrefab = default;

        [Header("Raycast Settings")]
        // [SerializeField]
        // [Tooltip("The maximum raycast distance.")]
        // private float maxQueryDistance = 300.0F;

        [Header("Tracer Settings")]
        // [SerializeField]
        // [Tooltip("The speed of the tracer graphics.")]
        [Range(1, 10)]
        private int tracerSpeed = 3;
        [SerializeField]
        [Tooltip("Should tracer graphics use gravity while moving?")]
        // private bool useGravity = true;
        // [SerializeField]
        // [Tooltip("If enabled, a random offset is applied to the spawn point to eliminate the \"Wagon-Wheel\" effect.")]
        // private bool applyStrobeOffset = true;

        private float _nextFireTime;
        private AudioSource audioSource; // Компонент для воспроизведения звука

        // Calculate tracer speed based on tracerSpeed value
        private float Speed => 10.0F + (tracerSpeed - 1) * 50.0F;

        private void Awake()
        {
            // Получаем или добавляем AudioSource
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
        }

        private void Update()
        {
            // Debug.Log(Vector3.Distance(transform.position, G.Player.transform.position));
            // if (Vector3.Distance(transform.position, G.Player.transform.position) <= 5f && !_isReload/* && Time.time >= _nextFireTime*/)
            // {
            // StartCoroutine(Fire());
            // _nextFireTime = Time.time + (1f / ShotsPerSecond);
            // }
        }
        
        public void Fire()
        {
            StartCoroutine(PerformShoot());
        }

        private IEnumerator PerformShoot()
        {
            if (_isReload)
                yield break;
            HitscanShot();
            _isReload = true;
            // Debug.Log("i am fiiiireeeee~~");
            yield return new WaitForSeconds(timeBetweenShots);
            _isReload = false;
            /// Determine wether we are sending a raycast, aka hitscan weapon, we are spawning a projectile or melee attacking
            // int style = (int)weapon.shootStyle;

            // weaponAnimator.StopWalkAndRunMotion();

            // Rest the bullets that have just been shot
            // reduceAmmo?.Invoke();

            // if (style == 1)
            // {
                // yield return new WaitForSeconds(weapon.shootDelay);
            // }


            //Determine weapon class / style
            // int i = 0;
            // while (i < bulletsPerFire)
            // {
                // if (weapon == null) yield break;
                // shooting = true;

                // CamShake.instance.ShootShake(camShakeAmount * aimingCamShakeMultiplier * crouchingCamShakeMultiplier);
                // if (weapon.useProceduralShot) ProceduralShot.Instance.Shoot(weapon.proceduralShotPattern);

                // Determine if we want to add an effect for FOV
                // if (weapon.applyFOVEffectOnShooting)
                // {
                    // float fovAdjustment = isAiming ? weapon.AimingFOVValueToSubtract : weapon.FOVValueToSubtract;
                    // cameraFOVManager.ForceAddFOV(-fovAdjustment);
                // }
                // foreach (var p in firePoint)
                // {
                    // if (muzzleVFX != null)
                        // Instantiate(p.position, transform.rotation, transform); // VFX
                // }
                // CowsinsUtilities.ForcePlayAnim("shooting", inventory[currentWeapon].GetComponentInChildren<Animator>());
                // if (weapon.timeBetweenShots > float.Epsilon) SoundManager.Instance.PlaySound(fireSFX, 0, weapon.pitchVariationFiringSFX, true, 0);

                // ProgressRecoil();

                // HitscanShot();
            // else if (style == 1) ProjectileShot();

                // i++;
            // }
            // shooting = false;
            // yield break;
        }


        private void HitscanShot()
        {
            // events.OnShoot.Invoke();
            // if (resizeCrosshair && UIController.instance.crosshair != null) UIController.instance.crosshair.Resize(weapon.crosshairResize * 10);

            Transform hitObj;

            //This defines the first hit on the object
            // Vector3 dir = CowsinsUtilities.GetSpreadDirection(spread, mainCamera);
            var dir = ImpactEffectTransform.forward;
            Ray ray = new Ray(ImpactEffectTransform.position, dir);

            if (Physics.Raycast(ray, out var hit, 15, hitLayer))
            {
                Debug.Log("target hitted");
                float dmg = damagePerBullet/* * multipliers.damageMultiplier*/;
                Hit(hit.collider.gameObject.layer, dmg, hit, true);
                hitObj = hit.collider.transform;

                if (hit.transform.TryGetComponent(out Rigidbody rb))
                {
                    rb.AddForceAtPosition(ray.direction * 15, hit.point, ForceMode.Impulse);
                }

                //Handle Penetration
                // Ray newRay = new Ray(hit.point, ray.direction);
                // RaycastHit newHit;

                // if (Physics.Raycast(newRay, out newHit, penetrationAmount, hitLayer))
                // {
                //     if (hitObj != newHit.collider.transform)
                //     {
                //         float dmg_ = damagePerBullet * multipliers.damageMultiplier * weapon.penetrationDamageReduction;
                //         Hit(newHit.collider.gameObject.layer, dmg_, newHit, true);
                //     }
                // }

                // Handle Bullet Trails
                // if (weapon.bulletTrail == null) return;

                // foreach (var p in firePoint)
                // {
                //     TrailRenderer trail = Instantiate(weapon.bulletTrail.gameObject, p.position, Quaternion.identity).GetComponent<TrailRenderer>();

                //     StartCoroutine(SpawnTrail(trail, hit));
                // }
            }
        }
        
        private void Hit(LayerMask layer, float damage, RaycastHit h, bool damageTarget)
        {
            // events.OnHit.Invoke();
            // GameObject impact = null, impactBullet = null;

            // Check the passed layer
            // If it matches any of the provided layers by FPS Engine, then:
            // Instantiate according effect and rotate it accordingly to the surface.
            // Instantiate bullet holes as well.
            // switch (layer)
            // {
            //     case int l when l == LayerMask.NameToLayer("Grass"):
            //         impact = PoolManager.Instance.GetFromPool(effects.grassImpact, h.point, Quaternion.LookRotation(h.normal)); // Grass
            //         if (weapon != null) impactBullet = PoolManager.Instance.GetFromPool(weapon.bulletHoleImpact.grassImpact, h.point, Quaternion.identity);
            //         break;
            //     case int l when l == LayerMask.NameToLayer("Metal"):
            //         impact = PoolManager.Instance.GetFromPool(effects.metalImpact, h.point, Quaternion.LookRotation(h.normal)); // Metal
            //         if (weapon != null) impactBullet = PoolManager.Instance.GetFromPool(weapon.bulletHoleImpact.metalImpact, h.point, Quaternion.identity);
            //         break;
            //     case int l when l == LayerMask.NameToLayer("Mud"):
            //         impact = PoolManager.Instance.GetFromPool(effects.mudImpact, h.point, Quaternion.LookRotation(h.normal)); // Mud
            //         if (weapon != null) impactBullet = PoolManager.Instance.GetFromPool(weapon.bulletHoleImpact.grassImpact, h.point, Quaternion.identity);
            //         break;
            //     case int l when l == LayerMask.NameToLayer("Wood"):
            //         impact = PoolManager.Instance.GetFromPool(effects.woodImpact, h.point, Quaternion.LookRotation(h.normal)); // Wood
            //         if (weapon != null) impactBullet = PoolManager.Instance.GetFromPool(weapon.bulletHoleImpact.woodImpact, h.point, Quaternion.identity);
            //         break;
            //     case int l when l == LayerMask.NameToLayer("Enemy"):
            //         impact = PoolManager.Instance.GetFromPool(effects.enemyImpact, h.point, Quaternion.LookRotation(h.normal)); // Enemy
            //         if (weapon != null) impactBullet = PoolManager.Instance.GetFromPool(weapon.bulletHoleImpact.enemyImpact, h.point, Quaternion.identity);
            //         break;
            //     default:
            //         impact = PoolManager.Instance.GetFromPool(effects.metalImpact, h.point, Quaternion.LookRotation(h.normal));
            //         if (weapon != null) impactBullet = PoolManager.Instance.GetFromPool(weapon.bulletHoleImpact.groundImpact, h.point, Quaternion.identity);
            //         break;
            // }

            // if (h.collider != null && impactBullet != null)
            // {
            //     impactBullet.transform.rotation = Quaternion.LookRotation(h.normal);
            //     impactBullet.transform.SetParent(h.collider.transform);
            // }

            // Apply damage
            if (!damageTarget)
            {
                return;
            }
            // float finalDamage = damage * GetDistanceDamageReduction(h.collider.transform);

            // Check if a head shot was landed
            if (h.collider.gameObject.CompareTag("Critical"))
            {
                CowsinsUtilities.GatherDamageableParent(h.collider.transform).Damage(damage, true/*finalDamage * weapon.criticalDamageMultiplier, true*/);
            }
            // Check if a body shot was landed ( for children colliders )
            else if (h.collider.gameObject.CompareTag("BodyShot"))
            {
                CowsinsUtilities.GatherDamageableParent(h.collider.transform).Damage(damage, false);
            }
            // Check if the collision just comes from the parent
            else if (h.collider.GetComponent<IDamageable>() != null)
            {
                Debug.Log("damage recieved");
                h.collider.GetComponent<IDamageable>().Damage(damage, false);
            }
        }

        private void OnCompleted(object sender, System.EventArgs e)
        {
            // Handle complete event here
            if (sender is TracerObject tracerObject)
            {
                Destroy(tracerObject.gameObject);
            }
        }

        private float CalculateStroboscopicOffset(float speed) => speed * Time.smoothDeltaTime;
    }
}