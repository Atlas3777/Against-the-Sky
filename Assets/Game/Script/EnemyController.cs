using UnityEngine;

[RequireComponent(typeof(CharacterBody))]
[RequireComponent(typeof(HeathSystem))]
[RequireComponent(typeof(CharacterController))]
public class EnemyController : MonoBehaviour, IEnemy
{
    public EnemyChaser EnemyChaser;
    public CharacterBody CharacterBody;
    public HeathSystem PlayerBody;
    public Transform Player;
    public HeathSystem Heath;
    public PlayerAnimation AnimationController;
    public CharacterController CharacterController;

    void Awake() => InitializeComponents();

    void InitializeComponents()
    {
        CharacterBody = GetComponent<CharacterBody>();
        Heath = GetComponent<HeathSystem>();
        CharacterController = GetComponent<CharacterController>();
        EnemyChaser = gameObject.AddComponent<EnemyChaser>();
    }

    void Start() => SetupComponents();

    void SetupComponents()
    {
        CharacterBody.Setup(Heath);
        Heath.Setup(CharacterBody);
        CharacterBody.jumpController?.Setup(CharacterController, AnimationController, transform);
    }

    void Update()
    {
        CharacterBody.jumpController?.UpdateJump();
    }

    public void Init(Transform player, CharacterBody playerBody)
    {
        Player = player;
        PlayerBody = playerBody.heathSystem;
        EnemyChaser.Init(player, playerBody);
    }
}
