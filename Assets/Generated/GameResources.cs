using cowsins.Inventory;
using cowsins.SaveLoad;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;

// This file is auto-generated. Do not modify manually.

public static class GameResources
{
    public static class HUD_Prefabs
    {
        public static GameObject Crosshair_outline => Resources.Load<GameObject>("HUD Prefabs/Crosshair outline");
        public static CanvasScaler HUD_Canvas => Resources.Load<CanvasScaler>("HUD Prefabs/HUD Canvas");
        public static Volume Vol => Resources.Load<Volume>("HUD Prefabs/Vol");
    }
    public static class MainCharacter
    {
        public static GameObject FPSController => Resources.Load<GameObject>("MainCharacter/FPSController");
        public static InventoryProManager InventoryController => Resources.Load<InventoryProManager>("MainCharacter/InventoryController");
    }
    public static class Map
    {
        public static Sprite map => Resources.Load<Sprite>("Map/map");
        public static MapManager MapManager => Resources.Load<MapManager>("Map/MapManager");
    }
    public static class Prefabs
    {
        public static InventoryItemPickeable ItemGenericPickeable => Resources.Load<InventoryItemPickeable>("Prefabs/ItemGenericPickeable");
    }
    public static EvacuationManager EvacuationManager => Resources.Load<EvacuationManager>("EvacuationManager");
    public static RigBuilder Kamikaze => Resources.Load<RigBuilder>("Kamikaze");
    public static LoadTrigger LoadTrigger => Resources.Load<LoadTrigger>("LoadTrigger");
    public static GameObject MovementCowsinsFPSController => Resources.Load<GameObject>("MovementCowsinsFPSController");
    public static RigBuilder Player => Resources.Load<RigBuilder>("Player");
    public static RigBuilder P_Enemy => Resources.Load<RigBuilder>("P_Enemy");
    public static SaveTrigger SaveTrigger => Resources.Load<SaveTrigger>("SaveTrigger");
    public static RigBuilder ShootingEnemy => Resources.Load<RigBuilder>("ShootingEnemy");
    public static SwitchAndSaveSceneTrigger SwitchSceneTrigger => Resources.Load<SwitchAndSaveSceneTrigger>("SwitchSceneTrigger");
    public static Timer Timer => Resources.Load<Timer>("Timer");
}
