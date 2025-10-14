using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace cowsins.SaveLoad
{
    // GameData contains all the Data that gets saved into an Encrypted Json File.
    [System.Serializable]
    public class GameData
    {
        // GLOBAL PARAMETERS
        public long lastUpdated;
        public string timeSaved;
        public string currentScene;

        // PLAYER TRANSFORMS
        public Dictionary<string, SerializablePlayerTransforms> playerTransforms; 

        // PLAYER STATISTICS
        public float playerHealth, playerShield;
        public float maxHealth, maxShield, damageMultiplier, healMultiplier;

        // EXPERIENCE + COINS
        public float totalExperience;
        public int playerLevel;
        public int coins;

        // CUSTOM SAVE DATA
        public Dictionary<string, CustomSaveData> triggeredObjects = new Dictionary<string, CustomSaveData>();
        public Dictionary<string, CustomSaveData> interactablesObjects = new Dictionary<string, CustomSaveData>();
        public Dictionary<string, CustomSaveData> enemyObjects = new Dictionary<string, CustomSaveData>();
        public Dictionary<string, CustomSaveData> destructibleObjects = new Dictionary<string, CustomSaveData>();
        public Dictionary<string, CustomSaveData> customObjects = new Dictionary<string, CustomSaveData>();
        public Dictionary<string, CustomSaveData> instantiatedObjects = new Dictionary<string, CustomSaveData>();
        // When creating custom dictionaries please make sure they are initialized.

        // PLAYER BASE INVENTORY ( HOTBAR )
        public int currentWeaponInt;
        public WeightSystem WeightSystem;

        // FPS ENGINE INVENTORY PRO ADD-ON
        public int rows, cols;
        public SerializableInventoryData[] serializableHotbarData;
        public SerializableInventoryData[] serializableInventoryData;
        public SerializableFavItemReference[] favItems;

        [System.Serializable]
        public class SerializableInventoryData
        {
            public string SOPath;
            public int amount;
            public int bulletsLeftInMagazine, totalBullets;
            public bool isOriented;
            // If Null, Ignore it ( don�t add to Json )
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string barrel,
                scope,
                stock,
                grip,
                magazine,
                flashlight,
                laser;
        }

        [System.Serializable]
        public class SerializableFavItemReference
        {
            public int row, col;
            public string itemName;
            public bool isInventorySlot;
        }

        [System.Serializable]
        public class SerializablePlayerTransforms
        {
            public float[] playerPosition;
            public float[] playerRotation;
            public bool isCrouching;

            public SerializablePlayerTransforms()
            {
                playerPosition = new float[3] { 0, 0, 0 };
                playerRotation = new float[4] { 0, 0, 0, 1 };
                isCrouching = false;
            }
        }

        // GameData Constructor
        public GameData()
        {
            playerTransforms = new Dictionary<string, SerializablePlayerTransforms>();
            playerHealth = 100f;
            playerShield = 50f;
            maxHealth = 100f;
            maxShield = 50f;
            WeightSystem = new WeightSystem(50, 0);
            damageMultiplier = 1f;
            healMultiplier = 1f;
            totalExperience = 0;
            playerLevel = 0;
            coins = 0;
            timeSaved = System.DateTime.Now.ToString();
            currentScene = "Level One";

            triggeredObjects = new Dictionary<string, CustomSaveData>();
            interactablesObjects = new Dictionary<string, CustomSaveData>();
            enemyObjects = new Dictionary<string, CustomSaveData>();
            customObjects = new Dictionary<string, CustomSaveData>();
            destructibleObjects = new Dictionary<string, CustomSaveData>();
            serializableHotbarData = new SerializableInventoryData[0];
            serializableInventoryData = new SerializableInventoryData[rows * cols];
            favItems = new SerializableFavItemReference[0];
        }
    }
}