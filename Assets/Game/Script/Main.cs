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
            controller.Init(Player, PlayerBody);
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
    public void Init(Transform player, CharacterBody playerBody);
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