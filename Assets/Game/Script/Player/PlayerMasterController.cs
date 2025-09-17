using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMasterController : MonoBehaviour
{
    [FormerlySerializedAs("movement")] [Header("Components")]
    public MovementComponent movementComponent;

    public PlayerCamera cameraController;
    public PlayerAim aimController;
    public PlayerAnimation animationController;
    public PlayerRig rigController;
    public PlayerAudio audioController;
    public CharacterBody characterBody;
    public HeathSystem heathSystem;

    [Header("Player References")] public GameObject CinemachineCameraTarget;
    public Transform aimTarget;
    public GameObject weapon;
    public Transform MoveTarget;

    private PlayerInput _playerInput;
    private CharacterController _controller;
    private BodyInputs _input;
    private GameObject _mainCamera;

    private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        SetupComponents();
        var r = GetComponent<NavMeshAgent>();
        r.updatePosition = false;
        r.updateRotation = false;
    }

    private void Update()
    {
        UpdateComponents();
        if (Input.GetMouseButtonDown(0))
        {
            NavMeshTest();
        }
    }

    private void LateUpdate()
    {
        cameraController?.LateUpdateCamera();
    }

    private void InitializeComponents()
    {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<BodyInputs>();
        _playerInput = GetComponent<PlayerInput>();

        // Инициализация компонентов
        if (movementComponent == null) movementComponent = gameObject.AddComponent<MovementComponent>();
        if (cameraController == null) cameraController = gameObject.AddComponent<PlayerCamera>();
        if (aimController == null) aimController = gameObject.AddComponent<PlayerAim>();
        if (animationController == null) animationController = gameObject.AddComponent<PlayerAnimation>();
        if (rigController == null) rigController = gameObject.AddComponent<PlayerRig>();
        if (audioController == null) audioController = gameObject.AddComponent<PlayerAudio>();
        if (characterBody == null) characterBody = gameObject.AddComponent<CharacterBody>();
        if (heathSystem == null) heathSystem = gameObject.AddComponent<HeathSystem>();
    }

    private void SetupComponents()
    {
        // Настройка всех компонентов
        movementComponent.Setup(_controller, _input, _mainCamera, animationController);
        cameraController.Setup(_input, CinemachineCameraTarget, IsCurrentDeviceMouse);
        aimController.Setup(_input, _mainCamera, aimTarget, rigController, weapon);
        animationController.Setup(GetComponent<Animator>(), _input);
        rigController.Setup(aimTarget);
        audioController.Setup(_controller);
        characterBody.Setup(heathSystem);
        characterBody.jumpController.Setup(_input, animationController, transform);
        heathSystem.Setup(characterBody);

        rigController.SetWeapon(weapon);

        // Передача ссылок между компонентами
        movementComponent.SetDependencies(characterBody.jumpController);
        Subs();
    }

    private void NavMeshTest()
    {
        var navMeshPath = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, MoveTarget.position, NavMesh.AllAreas, navMeshPath);
        Debug.Log(string.Join(",", navMeshPath.corners));
    }

    public void Subs()
    {
        characterBody.TakeDamageAction += (DamageInfo damage) =>
            G.HUD.SetHealth(heathSystem.CurrentHeath, heathSystem.MaxHealth);
        characterBody.Inventory.MoneyChanged += GGGG;
    }

    private void UpdateComponents()
    {
        aimController?.UpdateAim();
        rigController?.UpdateRigWeights(_input.aim || _input.shooting);
        cameraController?.UpdateCamera();
        characterBody.jumpController?.UpdateJump();
        movementComponent?.UpdateMovement();
        audioController?.UpdateAudio();
    }
    private void GGGG()
    {
        G.HUD.SetMoney(characterBody.Inventory.money);
        Debug.Log(characterBody.Inventory.money);
    }

    // private void OnDrawGizmosSelected()
    // {
    //     jumpController?.OnDrawGizmosSelected();
    // }

    private void OnDestroy()
    {
        // characterBody.TakeDamageAction -= UpdateUI;
        // characterBody.Inventory.MoneyChanged -= UpdateUI;
    }
}