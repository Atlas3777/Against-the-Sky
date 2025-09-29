using cowsins;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public Transform EnemySpawnPoint;
    public Transform Player;
    public GameObject PlayerStats;

    private void Start()
    {
        var enemy = Instantiate(EnemyPrefab, EnemySpawnPoint.position, Quaternion.identity);
        if (enemy.TryGetComponent<IEnemy>(out var controller))
        {
            controller.Init(Player, PlayerStats.GetComponent<PlayerStats>());
        }

        GlobalEventManager.BodyDeath += KillHandler;
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.R))
        //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void KillHandler(DeathInfo deathInfo)
    {
        deathInfo.attacker.Inventory.AddMoney(50);
    }
}

public interface IEnemy
{
    public void Init(Transform player, PlayerStats playerStats);
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