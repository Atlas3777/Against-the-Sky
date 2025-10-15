using UnityEngine;

[System.Serializable]
public class WeightSystem
{
    public float MaxWeight = 0;
    public float CurrentWeight;

    public WeightSystem(float maxWeight, float currentWeight)
    {
        this.MaxWeight = maxWeight;
        this.CurrentWeight = currentWeight;
    }

    public bool CanPickUp(ItemWeight itemWeight, int amountToAdd)
    {
        float addedWeight = itemWeight.Weight * amountToAdd;
        float newWeight = CurrentWeight + addedWeight;

        newWeight = (float)System.Math.Round(newWeight, 2); // <-- аккуратное округление

        if (newWeight <= MaxWeight)
        {
            CurrentWeight = newWeight;
            Debug.Log($"added weight: {itemWeight.Weight}; amount: {amountToAdd}; new weight: {CurrentWeight}");
            return true;
        }

        Debug.Log("too much weight");
        return false;
    }

    public void RemoveItems(ItemWeight itemWeight, int amountToRemove)
    {
        float removedWeight = itemWeight.Weight * Mathf.Abs(amountToRemove);
        float newWeight = Mathf.Max(0, CurrentWeight - removedWeight);

        CurrentWeight = (float)System.Math.Round(newWeight, 2); // <-- округляем после вычисления
        Debug.Log($"removed weight: {itemWeight.Weight}; amount: {Mathf.Abs(amountToRemove)}; new weight: {CurrentWeight}");
    }
}
