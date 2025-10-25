using cowsins;
using UnityEngine;

[DefaultExecutionOrder(-9999)]
public class Main : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public Transform EnemySpawnPoint;
    public GameObject Player;
    public GameObject DeadEnemyPrefab;
    public Transform DeadEnemySpawnPoint;

    private void Awake()
    {
        G.Player = Player;
        G.PlayerStats = G.Player.GetComponent<PlayerStats>();

        EnemyFactory.SpawnEnemy(EnemyPrefab, EnemySpawnPoint);

        GlobalEventManager.BodyDeath += KillHandler;
        Instantiate(DeadEnemyPrefab, DeadEnemySpawnPoint.position, Quaternion.identity);
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
    public void GetComponents();
    public void Init(GameObject target);
    public void MyStart();
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