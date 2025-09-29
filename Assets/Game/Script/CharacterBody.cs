using System;
using cowsins;
using UnityEngine;

public class CharacterBody : MonoBehaviour
{
    public EnemyHealth HealthSystem;
    
    public Inventory Inventory;

    public Action<DamageInfo> Death;
    public Action<DamageInfo> TakeDamageAction;
    public GravityController jumpController;

    public void TakeDamage(DamageInfo damage)
    {
        HealthSystem.Damage(damage.Damage, false);
    }
    
    private void DeathHandler(DamageInfo damage)
    {
        GlobalEventManager.BodyDeath.Invoke(new DeathInfo(damage.Attacker, damage.Target));
        gameObject.SetActive(false);
    }

    private void TakeDamageHandler(DamageInfo damage)
    {
        Debug.Log(gameObject.name + " is " + damage);
    }
    
    public void Setup(EnemyHealth enemyHealth)
    {
        if(Inventory != null)
            return;
        Inventory = new Inventory();
        this.HealthSystem = enemyHealth;
        jumpController = new GravityController();

        Death += DeathHandler;
        TakeDamageAction += TakeDamageHandler;
    }

    private void OnDestroy()
    {
        Death -= DeathHandler;
        TakeDamageAction -= TakeDamageHandler;
    }
}