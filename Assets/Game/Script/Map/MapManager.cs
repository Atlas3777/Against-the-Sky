using cowsins;
using UnityEngine;

public class MapManager : MonoBehaviour 
{
    private void Start()
    {
        InputManager.onMapOpenPressed += () => { print("m is pressed"); };
    }
}
