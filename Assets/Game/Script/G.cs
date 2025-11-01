using cowsins;
using cowsins.Inventory;
using cowsins.SaveLoad;
using UnityEngine;

public static class G
{
    public static GameObject Player;
    public static PlayerStats PlayerStats;
    public static InteractManager InteractManager;
    public static WeightSystem WeightSystem;
    public static GlobalActionManager  GlobalActionManager;
    
    public static MapManager MapManager;
    
    public static SoundManager SoundManager;
    public static GameSettingsManager GameSettingsManager;
    public static DataPersistenceManager DataPersistenceManager;
    public static GameDataManager GameDataManager;
    public static Main Main;
    public static InventoryProManager InventoryManager;
    
    public static SpawnPointManager SpawnPointManager;

    public static Camera MainCamera;
    public static Timer Timer;
    public static EvacuationManager EvacuationManager;
}