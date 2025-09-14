using System;
using UnityEngine;

public class CharacterBody : MonoBehaviour
{
    public HeathSystem heathSystem;
    
    public Inventory Inventory;

    public Action<DamageInfo> Death;
    public Action<DamageInfo> TakeDamageAction;

    public void TakeDamage(DamageInfo damage)
    {
        heathSystem.TakeDamage(damage);
    }

    private void Start()
    {
        Setup(GetComponent<HeathSystem>());
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
    
    public void Setup(HeathSystem heathSystem)
    {
        if(Inventory != null)
            return;
        Inventory = new Inventory();
        this.heathSystem = heathSystem;
        Death += DeathHandler;
        TakeDamageAction += TakeDamageHandler;
    }

    private void OnDestroy()
    {
        Death -= DeathHandler;
        TakeDamageAction -= TakeDamageHandler;
    }
}