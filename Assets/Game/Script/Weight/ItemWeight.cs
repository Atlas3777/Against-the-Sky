using UnityEngine;

[System.Serializable]
public class ItemWeight
{
    public float Weight = 1f;

    public ItemWeight(float weight) => this.Weight = weight;

    public void ChangeItemWeight(float delta) => Weight = Mathf.Max(Weight + delta, 0);
}
