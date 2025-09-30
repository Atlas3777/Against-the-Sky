using cowsins;
using UnityEngine;

[RequireComponent(typeof(CharacterBody))]
// [RequireComponent(typeof(HeathSystem))]
//[RequireComponent(typeof(CharacterController))]
public class EnemyController : MonoBehaviour, IEnemy
{
    public EnemyChaser EnemyChaser;
    public CharacterBody CharacterBody;
    public PlayerStats PlayerStats;
    public Transform Player;
    public EnemyHealth Health;
    public PlayerAnimation AnimationController;
    //public CharacterController CharacterController;

    void Awake() => InitializeComponents();

    void InitializeComponents()
    {
        CharacterBody = GetComponent<CharacterBody>();
        Health = GetComponent<EnemyHealth>();
        //CharacterController = GetComponent<CharacterController>();
        EnemyChaser = gameObject.AddComponent<EnemyChaser>();
    }

    void Start() => SetupComponents();

    void SetupComponents()
    {
        CharacterBody.Setup(Health);
        // Health.Start();
        //CharacterBody.jumpController?.Setup(CharacterController, AnimationController, transform);
    }

    void Update()
    {
        //CharacterBody.jumpController?.UpdateJump();
    }

    public void Init(Transform player, PlayerStats playerBody)
    {
        Player = player;
        PlayerStats = playerBody.GetComponent<PlayerStats>();
        EnemyChaser.Init(player, PlayerStats);
    }
}
