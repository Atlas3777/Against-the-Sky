using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Image image;
    public TMP_Text money;

    private void Start()
    {
        G.HUD = this;
    }

    public void SetHealth(float health, float maxHealth)
    {
        image.fillAmount = health / maxHealth;
        Debug.Log($"{health} / {maxHealth}");
    }
    

    public void SetMoney(int money)
    {
        this.money.text = money.ToString();
    }
}