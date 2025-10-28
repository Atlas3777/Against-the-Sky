using UnityEngine;

public interface IEnemy
{
    public void GetComponents();
    public void Init(GameObject target);
    public void MyStart();
}