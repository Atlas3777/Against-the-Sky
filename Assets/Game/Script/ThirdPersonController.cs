// using UnityEngine;
// using UnityEngine.Animations.Rigging;
// using UnityEngine.InputSystem;
//
// namespace StarterAssets
// {
//     [RequireComponent(typeof(CharacterController))]
//     [RequireComponent(typeof(PlayerInput))]
//     public class ThirdPersonController : MonoBehaviour
//     {
//         [Header("Player")] [Tooltip("Move speed of the character in m/s")]
//         public float MoveSpeed = 2.0f;
//
//         [Tooltip("Sprint speed of the character in m/s")]
//         public float SprintSpeed = 5.335f;
//
//         [Tooltip("How fast the character turns to face movement direction")] [Range(0.0f, 0.3f)]
//         public float RotationSmoothTime = 0.12f;
//
//         [Tooltip("Acceleration and deceleration")]
//         public float SpeedChangeRate = 10.0f;
//
//         public AudioClip LandingAudioClip;
//         public AudioClip[] FootstepAudioClips;
//         [Range(0, 1)] public float FootstepAudioVolume = 0.5f;
//
//         [Space(10)] [Tooltip("The height the player can jump")]
//         public float JumpHeight = 1.2f;
//
//         [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
//         public float Gravity = -15.0f;
//
//         [Space(10)]
//         [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
//         public float JumpTimeout = 0.50f;
//
//         [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
//         public float FallTimeout = 0.15f;
//
//         [Header("Player Grounded")]
//         [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
//         public bool Grounded = true;
//
//         [Tooltip("Useful for rough ground")] public float GroundedOffset = -0.14f;
//
//         [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
//         public float GroundedRadius = 0.28f;
//
//         [Tooltip("What layers the character uses as ground")]
//         public LayerMask GroundLayers;
//
//         [Header("Cinemachine")]
//         [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
//         public GameObject CinemachineCameraTarget;
//
//         [Tooltip("How far in degrees can you move the camera up")]
//         public float TopClamp = 70.0f;
//     
//         [Tooltip("How far in degrees can you move the camera down")]
//         public float BottomClamp = -30.0f;
//
//         [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
//         public float CameraAngleOverride = 0.0f;
//
//         [Tooltip("For locking the camera position on all axis")]
//         public bool LockCameraPosition = false;
//
//         [Header("Camera Gaze Settings")] [Tooltip("Maximum distance for the camera gaze raycast")]
//         public float MaxGazeDistance = 100f;
//
//         [Tooltip("Layers to consider for the gaze raycast")]
//         public LayerMask GazeLayers;
//
//         [Tooltip("The target GameObject that the upper body will aim towards.")]
//         public Transform aimTarget;
//
//         public Rig bodyRig;
//         public Rig weaponRig;
//         public Rig handRig;
//
//         public float AimRotationThreshold = 50.0f;
//
//         [Tooltip("Speed for interpolating rig weights")]
//         public float RigWeightChangeRate = 5.0f;
//
//         public GameObject weapon;
//
//         // cinemachine
//         private float _cinemachineTargetYaw;
//         private float _cinemachineTargetPitch;
//
//         // player
//         private float _speed;
//         private float _animationBlend;
//         private float _targetRotation = 0.0f;
//         private float _rotationVelocity;
//         private float _verticalVelocity;
//         private float _terminalVelocity = 53.0f;
//
//         // timeout deltatime
//         private float _jumpTimeoutDelta;
//         private float _fallTimeoutDelta;
//
//         // rig weights
//         private float _bodyRigTargetWeight;
//         private float _weaponRigTargetWeight;
//         private float _handRigTargetWeight;
//
//         // animation IDs
//         private int _animIDSpeed;
//         private int _animIDGrounded;
//         private int _animIDJump;
//         private int _animIDFreeFall;
//         private int _animIDMotionSpeed;
//         private int _animIDShoot;
//         private int _animIDIsTurning;
//         private int _animIDTurnDirection;
//
//         private PlayerInput _playerInput;
//         private Animator _animator;
//         private CharacterController _controller;
//         private StarterAssetsInputs _input;
//         private GameObject _mainCamera;
//
//         private const float _threshold = 0.01f;
//         private bool _hasAnimator;
//
//         private bool IsCurrentDeviceMouse
//         {
//             get { return _playerInput.currentControlScheme == "KeyboardMouse"; }
//         }
//
//         private void Awake()
//         {
//             if (_mainCamera == null)
//             {
//                 _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
//             }
//         }
//
//         private void Start()
//         {
//             _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
//             _hasAnimator = TryGetComponent(out _animator);
//             _controller = GetComponent<CharacterController>();
//             _input = GetComponent<StarterAssetsInputs>();
//             _playerInput = GetComponent<PlayerInput>();
//
//             AssignAnimationIDs();
//
//             _jumpTimeoutDelta = JumpTimeout;
//             _fallTimeoutDelta = FallTimeout;
//         }
//
//         private void Update()
//         {
//             _hasAnimator = TryGetComponent(out _animator);
//
//             // Определяем целевые веса ригов
//             _bodyRigTargetWeight = (_input.aim || _input.shooting) ? 1f : 0f;
//             _weaponRigTargetWeight = (_input.aim || _input.shooting) ? 1f : 0f;
//             _handRigTargetWeight = (_input.aim || _input.shooting) ? 1f : 0f;
//
//             if (_bodyRigTargetWeight == 1)
//             {
//                 weapon.SetActive(true);
//             }
//             else
//             {
//                 weapon.SetActive(false);
//             }
//
//
//             // Плавно интерполируем веса ригов
//             bodyRig.weight = Mathf.Lerp(bodyRig.weight, _bodyRigTargetWeight, Time.deltaTime * RigWeightChangeRate);
//             weaponRig.weight =
//                 Mathf.Lerp(weaponRig.weight, _weaponRigTargetWeight, Time.deltaTime * RigWeightChangeRate);
//             handRig.weight =
//                 Mathf.Lerp(handRig.weight, _handRigTargetWeight, Time.deltaTime * RigWeightChangeRate);
//
//             // Поворот при прицеливании/стрельбе
//             if (_input.aim || _input.shooting)
//             {
//                 Vector3 cameraForward = _mainCamera.transform.forward;
//                 cameraForward.y = 0f;
//                 cameraForward.Normalize();
//
//                 float angleToCamera = Vector3.Angle(transform.forward, cameraForward);
//                 if (angleToCamera > AimRotationThreshold)
//                 {
//                     _targetRotation = Mathf.Atan2(cameraForward.x, cameraForward.z) * Mathf.Rad2Deg;
//                     float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
//                         ref _rotationVelocity, RotationSmoothTime);
//                     transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
//                 }
//             }
//
//             ShootCheck();
//             JumpAndGravity();
//             GroundedCheck();
//             Move();
//             UpdateGazeTarget();
//         }
//
//         private void UpdateGazeTarget()
//         {
//             if (!aimTarget || !_mainCamera)
//                 return;
//
//             Vector3 cameraPosition = _mainCamera.transform.position;
//             Vector3 cameraForward = _mainCamera.transform.forward;
//
//             Ray ray = new Ray(cameraPosition, cameraForward);
//             RaycastHit hit;
//
//             Vector3 targetPosition;
//
//             if (Physics.Raycast(ray, out hit, MaxGazeDistance, GazeLayers))
//             {
//                 targetPosition = hit.point;
//             }
//             else
//             {
//                 targetPosition = cameraPosition + cameraForward * MaxGazeDistance;
//             }
//
//             aimTarget.position = targetPosition;
//         }
//
//         private void ShootCheck()
//         {
//             _animator.SetBool(_animIDShoot, _input.shooting);
//         }
//
//         private void LateUpdate()
//         {
//             CameraRotation();
//         }
//
//         private void AssignAnimationIDs()
//         {
//             _animIDSpeed = Animator.StringToHash("Speed");
//             _animIDGrounded = Animator.StringToHash("Grounded");
//             _animIDJump = Animator.StringToHash("Jump");
//             _animIDFreeFall = Animator.StringToHash("FreeFall");
//             _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
//             _animIDShoot = Animator.StringToHash("Shoot");
//             _animIDIsTurning = Animator.StringToHash("IsTurning");
//             _animIDTurnDirection = Animator.StringToHash("TurnDirection");
//         }
//
//         private void GroundedCheck()
//         {
//             Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
//                 transform.position.z);
//             Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
//                 QueryTriggerInteraction.Ignore);
//
//             if (_hasAnimator)
//             {
//                 _animator.SetBool(_animIDGrounded, Grounded);
//             }
//         }
//
//         private void CameraRotation()
//         {
//             if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
//             {
//                 float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
//
//                 _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
//                 _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
//             }
//
//             _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
//             _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
//
//             CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
//                 _cinemachineTargetYaw, 0.0f);
//         }
//
//         private void Move()
//         {
//             float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
//             if (_input.move == Vector2.zero)
//                 targetSpeed = 0f;
//
//             _speed = Mathf.Lerp(_speed, targetSpeed, Time.deltaTime * SpeedChangeRate);
//
//             Vector3 forward = _mainCamera.transform.forward;
//             Vector3 right = _mainCamera.transform.right;
//
//             forward.y = 0f;
//             right.y = 0f;
//
//             forward.Normalize();
//             right.Normalize();
//
//             Vector3 moveDirection = forward * _input.move.y + right * _input.move.x;
//             Vector3 velocity = moveDirection.normalized * _speed;
//             velocity.y = _verticalVelocity;
//
//             // Локальное направление движения для Blend Tree
//             Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);
//             float animVertical = localMoveDirection.z;
//             float animHorizontal = localMoveDirection.x;
//
//             // Масштабируем параметры анимации на основе текущей скорости (множитель = 1 при ходьбе, ≈2.667 при беге)
//             float multiplier = _speed / MoveSpeed; // Автоматически учитывает lerp скорости и sprint
//
//             animVertical *= multiplier;
//             animHorizontal *= multiplier;
//
//             bool isIdle = _speed < 0.1f && moveDirection.sqrMagnitude < 0.01f;
//             bool isAiming = _input.aim || _input.shooting;
//
//             if (isAiming)
//             {
//                 if (isIdle)
//                 {
//                     // Turn-in-place как раньше
//                     Vector3 cameraForward = _mainCamera.transform.forward;
//                     cameraForward.y = 0f;
//                     cameraForward.Normalize();
//
//                     float turnAngle = Mathf.DeltaAngle(transform.eulerAngles.y, _mainCamera.transform.eulerAngles.y);
//
//                     if (Mathf.Abs(turnAngle) > AimRotationThreshold)
//                     {
//                         _animator.SetBool(_animIDIsTurning, true);
//                         _animator.SetFloat(_animIDTurnDirection, Mathf.Sign(turnAngle));
//
//                         _targetRotation = _mainCamera.transform.eulerAngles.y;
//                         float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
//                             ref _rotationVelocity, RotationSmoothTime);
//                         transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
//                     }
//                     else
//                     {
//                         _animator.SetBool(_animIDIsTurning, false);
//                     }
//                 }
//                 else
//                 {
//                     // При движении — поворачиваем только если движемся вперёд
//                     float forwardAmount = Vector3.Dot(moveDirection.normalized, transform.forward);
//
//                     if (forwardAmount > 0f) // Вперёд
//                     {
//                         _targetRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
//                         float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
//                             ref _rotationVelocity, RotationSmoothTime);
//                         transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
//                     }
//                     // Если назад — не трогаем rotation, оставляем взгляд вперёд
//                 }
//             }
//             else
//             {
//                 _animator.SetBool(_animIDIsTurning, false);
//
//                 if (moveDirection != Vector3.zero)
//                 {
//                     _targetRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
//                     float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
//                         ref _rotationVelocity, RotationSmoothTime);
//                     transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
//                 }
//             }
//
//
//             // Передаём параметры в Animator
//             if (_hasAnimator)
//             {
//                 _animator.SetFloat("Vertical", animVertical, 0.1f, Time.deltaTime);
//                 _animator.SetFloat("Horizontal", animHorizontal, 0.1f, Time.deltaTime);
//                 _animator.SetFloat("Speed", _speed);
//             }
//
//             // Двигаем контроллер
//             _controller.Move(velocity * Time.deltaTime);
//         }
//
//         private void JumpAndGravity()
//         {
//             if (Grounded)
//             {
//                 _fallTimeoutDelta = FallTimeout;
//
//                 if (_hasAnimator)
//                 {
//                     _animator.SetBool(_animIDJump, false);
//                     _animator.SetBool(_animIDFreeFall, false);
//                 }
//
//                 if (_verticalVelocity < 0.0f)
//                     _verticalVelocity = -2f;
//
//                 if (_input.jump && _jumpTimeoutDelta <= 0.0f)
//                 {
//                     _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
//
//                     if (_hasAnimator)
//                         _animator.SetBool(_animIDJump, true);
//                 }
//
//                 if (_jumpTimeoutDelta >= 0.0f)
//                 {
//                     _jumpTimeoutDelta -= Time.deltaTime;
//                 }
//             }
//             else
//             {
//                 _jumpTimeoutDelta = JumpTimeout;
//                 if (_fallTimeoutDelta >= 0.0f)
//                     _fallTimeoutDelta -= Time.deltaTime;
//                 else
//                     _animator.SetBool(_animIDFreeFall, true);
//
//
//                 _input.jump = false;
//             }
//
//             if (_verticalVelocity < _terminalVelocity)
//                 _verticalVelocity += Gravity * Time.deltaTime;
//         }
//
//         private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
//         {
//             if (lfAngle < -360f) lfAngle += 360f;
//             if (lfAngle > 360f) lfAngle -= 360f;
//             return Mathf.Clamp(lfAngle, lfMin, lfMax);
//         }
//
//         private void OnDrawGizmosSelected()
//         {
//             Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
//             Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
//
//             if (Grounded) Gizmos.color = transparentGreen;
//             else Gizmos.color = transparentRed;
//
//             Gizmos.DrawSphere(
//                 new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
//                 GroundedRadius);
//         }
//
//         private void OnFootstep(AnimationEvent animationEvent)
//         {
//             if (animationEvent.animatorClipInfo.weight > 0.5f)
//             {
//                 if (FootstepAudioClips.Length > 0)
//                 {
//                     var index = Random.Range(0, FootstepAudioClips.Length);
//                     AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center),
//                         FootstepAudioVolume);
//                 }
//             }
//         }
//
//         private void OnLand(AnimationEvent animationEvent)
//         {
//             if (animationEvent.animatorClipInfo.weight > 0.5f)
//             {
//                 AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center),
//                     FootstepAudioVolume);
//             }
//         }
//     }
// }