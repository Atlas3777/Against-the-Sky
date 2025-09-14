using UnityEngine;

public class HeathSystem : MonoBehaviour
{
    public CharacterBody characterBody;
    public float MaxHealth;
    public float CurrentHeath;

    public void Setup(CharacterBody characterBody)
    {
        this.characterBody = characterBody;
    }

    public void TakeDamage(DamageInfo damage)
    {
        CurrentHeath -= damage.Damage;
        characterBody.TakeDamageAction.Invoke(damage);
        if (CurrentHeath <= 0)
        {
            CurrentHeath = 0;
            characterBody.Death.Invoke(damage);
        }
    }
}