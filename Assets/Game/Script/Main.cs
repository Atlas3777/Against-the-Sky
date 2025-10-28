using System.Collections;
using cowsins;
using UnityEngine;

[DefaultExecutionOrder(-5000)]
public class Main : MonoBehaviour
{
    private void Awake()
    {
        G.Player = Instantiate(GameResources.MainCharacter.FPSController, G.SpawnPointManager.GetSpawnPosition().position, Quaternion.identity)
            .GetComponentInChildren<PlayerMovement>().gameObject;
        G.InventoryManager = Instantiate(GameResources.MainCharacter.InventoryController);
        G.MapManager = Instantiate(GameResources.Map.MapManager);
    }

    private void Start()
    {
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.5f);
    }
}