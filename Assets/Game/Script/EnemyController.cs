// using cowsins;
// using UnityEngine;
//
// [RequireComponent(typeof(CharacterBody))]
// // [RequireComponent(typeof(HeathSystem))]
// //[RequireComponent(typeof(CharacterController))]
// public class EnemyController : MonoBehaviour, IEnemy
// {
//     public EnemyChaser EnemyChaser;
//     public CharacterBody CharacterBody;
//     public PlayerStats PlayerStats;
//     public Transform Player;
//     public EnemyHealth Health;
//     public PlayerAnimation AnimationController;
//
//     void Awake() => InitializeComponents();
//
//     void InitializeComponents()
//     {
//         CharacterBody = GetComponent<CharacterBody>();
//         Health = GetComponent<EnemyHealth>();
//     }
//
//     void Start() => SetupComponents();
//
//     void SetupComponents()
//     {
//         CharacterBody.Setup(Health);
//     }
//
//     void Update()
//     {
//         //CharacterBody.jumpController?.UpdateJump();
//     }
//
//     public void Init(GameObject player , ActionSpot spot)
//     {
//         EnemyChaser.Init(player, spot);
//     }
// }
