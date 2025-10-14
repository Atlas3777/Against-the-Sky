using UnityEngine;

[System.Serializable]
public class WeightSystem
{
    public float MaxWeight;
    public float CurrentWeight;

    public WeightSystem(float maxWeight, float currentWeight)
    {
        this.MaxWeight = maxWeight;
        this.CurrentWeight = currentWeight;
    }

    public bool CanPickUp(ItemWeight itemWeight, int amountToAdd)
    {
        if (CurrentWeight + (itemWeight.Weight * amountToAdd) <= MaxWeight)
        {
            CurrentWeight += itemWeight.Weight * amountToAdd;
            Debug.Log($"added weight: {itemWeight.Weight}; amount: {amountToAdd}; new weight: {CurrentWeight}");
            return true;
        }
        Debug.Log("too much weight");
        return false;
    }

    public void RemoveItems(ItemWeight itemWeight, int amountToRemove)
    {
        CurrentWeight = Mathf.Max(0, CurrentWeight - (itemWeight.Weight * Mathf.Abs(amountToRemove)));
            Debug.Log($"removed weight: {itemWeight.Weight}; amount: {Mathf.Abs(amountToRemove)}; new weight: {CurrentWeight}");
    }
}
