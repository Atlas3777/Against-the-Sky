using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public Transform EnemySpawnPoint;
    public Transform Player;
    public CharacterBody PlayerBody;

    private void Start()
    {
        var enemy = Instantiate(EnemyPrefab, EnemySpawnPoint.position, Quaternion.identity);
        if (enemy.TryGetComponent<IEnemy>(out var controller))
        {
<<<<<<< refs/remotes/origin/fixed_enemy_refactor
            controller.Init(Player, PlayerBody);
=======
            controller.Init(Player, PlayerStats.GetComponent<PlayerStats>());
            controller.AAAAAAStart();
>>>>>>> local
        }

        GlobalEventManager.BodyDeath += KillHandler;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void KillHandler(DeathInfo deathInfo)
    {
        deathInfo.attacker.Inventory.AddMoney(50);
    }
}

public interface IEnemy
{
<<<<<<< refs/remotes/origin/fixed_enemy_refactor
    public void Init(Transform player, CharacterBody playerBody);
=======
    public void Init(Transform player, PlayerStats playerStats);
    public void AAAAAAStart();
>>>>>>> local
}

public class DamageInfo
{
    public DamageInfo(float damage, CharacterBody attacker,CharacterBody target)
    {
        Damage = damage;
        Attacker = attacker;
        Target = target;
    }

    public float Damage;
    public CharacterBody Attacker;
    public CharacterBody Target;
}