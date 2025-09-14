using System;
using UnityEngine;

public class Inventory
{
    public int money { get; private set; }
    public Action MoneyChanged;

    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log("AddMoney Context " + money);
        MoneyChanged?.Invoke();
    }
}