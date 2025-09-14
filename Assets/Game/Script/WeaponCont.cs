using Andtech.ProTracer;
using StarterAssets;
using UnityEngine;

namespace Game.Script
{
    public class WeaponCont : MonoBehaviour
    {
        public CharacterBody characterBody;
        public BodyInputs inputs;
        public float ShotsPerSecond = 7.0f; // Matches rateOfFire from TracerDemo
        public GameObject ImpactEffect; // Эффект у ствола
        public GameObject HitEffect; // Эффект попадания
        public Transform ImpactEffectTransform;
        public AudioClip shootSound; // Аудиоклип для звука выстрела
        public AudioClip impactShootSound; // для попадания

        [Header("Tracer Prefabs")]
        [SerializeField]
        [Tooltip("The Bullet prefab to spawn.")]
        private Bullet bulletPrefab = default;
        [SerializeField]
        [Tooltip("The Smoke Trail prefab to spawn.")]
        private SmokeTrail smokeTrailPrefab = default;

        [Header("Raycast Settings")]
        [SerializeField]
        [Tooltip("The maximum raycast distance.")]
        private float maxQueryDistance = 300.0F;

        [Header("Tracer Settings")]
        [SerializeField]
        [Tooltip("The speed of the tracer graphics.")]
        [Range(1, 10)]
        private int tracerSpeed = 3;
        [SerializeField]
        [Tooltip("Should tracer graphics use gravity while moving?")]
        private bool useGravity = true;
        [SerializeField]
        [Tooltip("If enabled, a random offset is applied to the spawn point to eliminate the \"Wagon-Wheel\" effect.")]
        private bool applyStrobeOffset = true;

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
            characterBody = GetComponentInParent<CharacterBody>();
        }

        private void Update()
        {
            if (inputs.shooting && Time.time >= _nextFireTime)
            {
                Fire();
                _nextFireTime = Time.time + (1f / ShotsPerSecond);
            }
        }

        private void Fire()
        {
            // Проигрывание звука выстрела
            if (shootSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shootSound);
            }

            // Создание эффекта у ствола
            if (ImpactEffect != null)
            {
                var impactEffectInstance = Instantiate(ImpactEffect, ImpactEffectTransform.position, ImpactEffectTransform.rotation);
                Destroy(impactEffectInstance, 4);
            }

            // Compute tracer parameters
            float speed = Speed;
            float offset = applyStrobeOffset ? Random.Range(0.0F, CalculateStroboscopicOffset(speed)) : 0.0F;

            // Instantiate the tracer graphics
            Bullet bullet = Instantiate(bulletPrefab);
            SmokeTrail smokeTrail = Instantiate(smokeTrailPrefab);

            // Setup callbacks
            bullet.Completed += OnCompleted;
            smokeTrail.Completed += OnCompleted;

            // Use different tracer drawing methods depending on the raycast
            if (Physics.Raycast(ImpactEffectTransform.position, ImpactEffectTransform.forward, out RaycastHit hitInfo, maxQueryDistance))
            {
                // Since start and end point are known, use DrawLine
                bullet.DrawLine(ImpactEffectTransform.position, hitInfo.point, speed, offset);
                smokeTrail.DrawLine(ImpactEffectTransform.position, hitInfo.point, speed, offset);

                // Setup impact callback
                bullet.Arrived += OnImpact;

                void OnImpact(object sender, System.EventArgs e)
                {
                    // Создание эффекта попадания в точке контакта
                    if (HitEffect != null)
                    {
                        var hitEffectInstance = Instantiate(HitEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                        Destroy(hitEffectInstance, 4);
                        
                        AudioSource.PlayClipAtPoint(impactShootSound, hitInfo.point);
                    }

                    if (hitInfo.collider.gameObject.TryGetComponent<CharacterBody>(out var bodyTarget))
                    {
                        bodyTarget.TakeDamage(new DamageInfo(10, characterBody, bodyTarget));
                    }
                    
                    Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red, 0.5F);
                }
            }
            else
            {
                // Since we have no end point, use DrawRay
                bullet.DrawRay(ImpactEffectTransform.position, ImpactEffectTransform.forward, speed, maxQueryDistance, offset, useGravity);
                smokeTrail.DrawRay(ImpactEffectTransform.position, ImpactEffectTransform.forward, speed, 25.0F, offset);
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