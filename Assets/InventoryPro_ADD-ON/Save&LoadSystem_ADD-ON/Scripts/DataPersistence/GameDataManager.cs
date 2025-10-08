using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static cowsins.SaveLoad.GameData;


#if INVENTORY_PRO_ADD_ON
using cowsins.Inventory;
#endif

namespace cowsins.SaveLoad
{
    public class GameDataManager : MonoBehaviour
    {
        // REFERENCES
        public static GameDataManager instance { get; private set; }
        private WeaponController weaponController;
        private PlayerMovement playerMovement;
        private PlayerStats playerStats;
        private PlayerMultipliers playerMultipliers;
        private PlayerStates playerStates;
        private UIController uiController;
        private ExperienceManager experienceManager;
        private CoinManager coinManager;

        // SAVE DATA
        // These dictionaries store the information before assigning it to the GameData and save it.
        public Dictionary<string, CustomSaveData> triggersData = new Dictionary<string, CustomSaveData>();
        public Dictionary<string, CustomSaveData> interactablesData = new Dictionary<string, CustomSaveData>();
        public Dictionary<string, CustomSaveData> enemyHealthData = new Dictionary<string, CustomSaveData>();
        public Dictionary<string, CustomSaveData> destructiblesData = new Dictionary<string, CustomSaveData>();
        public Dictionary<string, CustomSaveData> customObjectsData = new Dictionary<string, CustomSaveData>();
        public Dictionary<string, CustomSaveData> instantiatedObjects = new Dictionary<string, CustomSaveData>();

        // PREFABS & TYPE TO PREFAB MAPPING
        [System.Serializable]
        public class TypeToPrefabMapping
        {
            // This matches the "type" in your save data
            public string TypeName;
            // The prefab to instantiate for this type
            public Identifiable Prefab;
        }

        [SerializeField] private List<TypeToPrefabMapping> typeToPrefabMappings;
        private Dictionary<string, Identifiable> prefabDictionary;

        #region Initialization
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            instance = this;
            PopulatePrefabDictionary();
        }

        private void OnEnable() => SceneManager.activeSceneChanged += OnSceneLoaded;
        private void OnDisable() => SceneManager.activeSceneChanged -= OnSceneLoaded;

        private void OnSceneLoaded(Scene oldScene, Scene newScene)
        {
            // Gather References
#pragma warning disable CS0618
            playerMovement = FindObjectOfType<PlayerMovement>();
#pragma warning restore CS0618
            weaponController = playerMovement?.GetComponent<WeaponController>();
            playerStats = playerMovement?.GetComponent<PlayerStats>();
            playerMultipliers = playerMovement?.GetComponent<PlayerMultipliers>();
            playerStates = playerMovement?.GetComponent<PlayerStates>();

            bool isInventoryAvailable = false;
#if INVENTORY_PRO_ADD_ON
            isInventoryAvailable = InventoryProManager.instance != null;
#endif
            if (playerMovement != null && !isInventoryAvailable)
            {
                playerMovement.GetComponent<InteractManager>().events.onDrop.RemoveAllListeners();
                playerMovement.GetComponent<InteractManager>().events.onDrop.AddListener(OnDropHotbar);
            }

            uiController = UIController.instance;
            experienceManager = ExperienceManager.instance;
            coinManager = CoinManager.Instance;

            // Since we loaded a new scene, previous dictionary references are no longer valid, we need to reset them
            ResetGameDataDictionaries();
        }

        private void ResetGameDataDictionaries()
        {
            triggersData = new Dictionary<string, CustomSaveData>();
            interactablesData = new Dictionary<string, CustomSaveData>();
            enemyHealthData = new Dictionary<string, CustomSaveData>();
            destructiblesData = new Dictionary<string, CustomSaveData>();
            customObjectsData = new Dictionary<string, CustomSaveData>();
            instantiatedObjects = new Dictionary<string, CustomSaveData>();
        }

        private void PopulatePrefabDictionary() => prefabDictionary = typeToPrefabMappings.ToDictionary(m => m.TypeName, m => m.Prefab);
        #endregion

        #region Save
        public void SaveData(GameData gameData, DataPersistence_SO dataPersistenceSettings)
        {
            // If Data Persistence Settings is null, return.
            // Data Persistence Settings ( Scriptable Object ) must be properly assigned in the Data Persistence Manager object.
            if (dataPersistenceSettings == null)
            {
                Debug.LogError("<color=red>[COWSINS]</color> <b><color=yellow>Data Persistence Settings not found!</color></b> " +
                    "Please configure the <b><color=cyan>Data Persistence Settings file</color></b> and assign it in the " +
                    "<b><color=cyan>Data Persistence Manager</color></b> object to fix this error.");
                return;
            }

            // If no Player is found, or the player is dead, avoid saving
            if (playerMovement == null || playerStats?.IsDead == true) return;

            // Save Global Parameters: Scene, Date & Time
            string sceneName = SceneManager.GetActiveScene().name;
            // We need to check if the scene is excluded from saving specific data, like Player Transforms & Custom Object Data
            bool isSceneExcluded = DataPersistenceManager.instance.IsSceneExcludedFromSpecificData();
            gameData.timeSaved = System.DateTime.Now.ToString("dd MMMM, yyyy HH:mm");
            if (!isSceneExcluded) gameData.currentScene = sceneName;

            // Save
            SavePlayerData(gameData, dataPersistenceSettings, sceneName, isSceneExcluded);
#if INVENTORY_PRO_ADD_ON
            if (dataPersistenceSettings.SaveInventory) SaveInventoryData(gameData);
#endif
            if (dataPersistenceSettings.SaveObjects && !isSceneExcluded) SaveObjectsData(gameData);
        }

        private void SavePlayerData(GameData gameData, DataPersistence_SO dataPersistenceSettings, string sceneName, bool isSceneExcluded)
        {
            if (dataPersistenceSettings.SavePlayerTransforms && !isSceneExcluded)
            {
                SerializablePlayerTransforms transformData = new SerializablePlayerTransforms
                {
                    // Save Position
                    playerPosition = new float[]
                    {
                        playerMovement.transform.position.x,
                        playerMovement.transform.position.y,
                        playerMovement.transform.position.z
                    },
                    // Save Orientation Quaternion
                    playerRotation = new float[]
                    {
                        playerMovement.orientation.transform.rotation.x,
                        playerMovement.orientation.transform.rotation.y,
                        playerMovement.orientation.transform.rotation.z,
                        playerMovement.orientation.transform.rotation.w
                    },
                    // Save Crouch State
                    isCrouching = playerMovement.isCrouching
                };

                // Add or update the dictionary entry accordingly
                if (gameData.playerTransforms.ContainsKey(sceneName))
                {
                    gameData.playerTransforms[sceneName] = transformData;
                }
                else
                {
                    gameData.playerTransforms.Add(sceneName, transformData);
                }
            }

            if (dataPersistenceSettings.SavePlayerStats)
            {
                // Save Health & Shield
                gameData.playerHealth = playerStats.health;
                gameData.playerShield = playerStats.shield;
                gameData.maxHealth = playerStats.maxHealth;
                gameData.maxShield = playerStats.maxShield;

                // Save Player Multipliers
                gameData.damageMultiplier = playerMultipliers.damageMultiplier;
                gameData.healMultiplier = playerMultipliers.healMultiplier;
            }

            if (dataPersistenceSettings.SaveExperience)
            {
                // Save XP & Level
                gameData.totalExperience = experienceManager.TotalExperience;
                gameData.playerLevel = experienceManager.playerLevel;
            }

            // Save Currency
            if (dataPersistenceSettings.SaveCoins)
                gameData.coins = coinManager.coins;

            // Save Hotbar ( FPS Engine�s Base Inventory )
            if (dataPersistenceSettings.SaveHotbar)
            {
                // Save current weapon unholstered
                gameData.currentWeaponInt = weaponController.currentWeapon;

                // For each weapon, we need to save their name, bullets, and attachments.
                int hotbarSize = weaponController.inventory.Length;
                gameData.serializableHotbarData = new GameData.SerializableInventoryData[hotbarSize];
                for (int i = 0; i < hotbarSize; i++)
                {
                    GameData.SerializableInventoryData tempHotbarData = new GameData.SerializableInventoryData();

                    if (weaponController.inventory[i] != null)
                    {
                        WeaponIdentification weaponIdentification = weaponController.inventory[i];
                        tempHotbarData.SOPath = weaponIdentification.weapon.name;
                        tempHotbarData.amount = 1;
                        tempHotbarData.bulletsLeftInMagazine = weaponIdentification.bulletsLeftInMagazine;
                        tempHotbarData.totalBullets = weaponIdentification.totalBullets;

                        tempHotbarData.barrel = weaponIdentification.barrel?.attachmentIdentifier.name;
                        tempHotbarData.scope = weaponIdentification.scope?.attachmentIdentifier.name;
                        tempHotbarData.stock = weaponIdentification.stock?.attachmentIdentifier.name;
                        tempHotbarData.grip = weaponIdentification.grip?.attachmentIdentifier.name;
                        tempHotbarData.magazine = weaponIdentification.magazine?.attachmentIdentifier.name;
                        tempHotbarData.flashlight = weaponIdentification.flashlight?.attachmentIdentifier.name;
                        tempHotbarData.laser = weaponIdentification.laser?.attachmentIdentifier.name;
                    }
                    else
                    {
                        tempHotbarData.SOPath = string.Empty;
                        tempHotbarData.amount = 0;
                        tempHotbarData.bulletsLeftInMagazine = 0;
                        tempHotbarData.totalBullets = 0;
                    }

                    gameData.serializableHotbarData[i] = tempHotbarData;

                }
            }
        }
#if INVENTORY_PRO_ADD_ON
        private void SaveInventoryData(GameData gameData)
        {
            // if the Inventory can�t be found, return
            if (InventoryProManager.instance == null) return;

            // Store size parameters
            gameData.rows = InventoryProManager.instance._GridGenerator.Rows;
            gameData.cols = InventoryProManager.instance._GridGenerator.Columns;
            gameData.serializableInventoryData = new GameData.SerializableInventoryData[gameData.rows * gameData.cols];

            // Store the SlotData for each slot in the Inventory.
            for (int i = 0; i < gameData.rows * gameData.cols; i++)
            {
                GameData.SerializableInventoryData tempInvData = new GameData.SerializableInventoryData();

                // Calculate row and column indices from the 1D index i
                int row = i / gameData.cols;
                int col = i % gameData.cols;

                // Access the item based on the calculated row and column
                SlotData slotData = InventoryProManager.instance.InventorySlots[row, col].slotData;

                tempInvData.SOPath = slotData.amount > 0 && slotData.item != null ? slotData.item.name : string.Empty;
                tempInvData.isOriented = slotData.isOriented;
                tempInvData.amount = slotData.amount;
                tempInvData.bulletsLeftInMagazine = slotData.bulletsLeftInMagazine;
                tempInvData.totalBullets = slotData.totalBullets;

                tempInvData.barrel = slotData.barrel?.name;
                tempInvData.scope = slotData.scope?.name;
                tempInvData.stock = slotData.stock?.name;
                tempInvData.grip = slotData.grip?.name;
                tempInvData.magazine = slotData.magazine?.name;
                tempInvData.flashlight = slotData.flashlight?.name;
                tempInvData.laser = slotData.laser?.name;

                gameData.serializableInventoryData[i] = tempInvData;
            }

            // If the Inventory does not support the Favorites Pinned Radial menu, return
            if (!InventoryProManager.instance.UseFavouritesRadialMenu) return;

            // Save size parameters
            int favSize = InventoryProManager.instance._FavItemsMenu.RadialMenuSize;
            if (favSize <= 0) return;

            // Store Fav Items References
            gameData.favItems = new GameData.SerializableFavItemReference[favSize];
            for (int i = 0; i < favSize; i++)
            {
                FavItemReference favItemReference = InventoryProManager.instance._FavItemsMenu.favoritesList[i];
                GameData.SerializableFavItemReference tempFavItem = new GameData.SerializableFavItemReference
                {
                    row = favItemReference.row,
                    col = favItemReference.col,
                    itemName = favItemReference.item?.name,
                    isInventorySlot = favItemReference.isInventorySlot
                };

                gameData.favItems[i] = tempFavItem;
            }
        }
#endif
        private void SaveObjectsData(GameData gameData)
        {
            // Save triggered objects as a dictionary
            gameData.triggeredObjects = triggersData;

            // Save interactables as a dictionary
            gameData.interactablesObjects = interactablesData;

            gameData.enemyObjects = enemyHealthData;

            gameData.customObjects = customObjectsData;

            gameData.destructibleObjects = destructiblesData;
            gameData.instantiatedObjects = instantiatedObjects;
        }

        #endregion

        #region Load
        public void LoadData(GameData gameData, DataPersistence_SO dataPersistenceSettings)
        {
            // If Data Persistence Settings is null, return.
            // Data Persistence Settings ( Scriptable Object ) must be properly assigned in the Data Persistence Manager object.
            if (dataPersistenceSettings == null)
            {
                Debug.LogError("<color=red>[COWSINS]</color> <b><color=yellow>Data Persistence Settings not found!</color></b> " +
                    "Please configure the <b><color=cyan>Data Persistence Settings file</color></b> and assign it in the " +
                    "<b><color=cyan>Data Persistence Manager</color></b> object to fix this error.");
                return;
            }
            // If Player is not found in the scene, return
            if (playerMovement == null) return;

            string currentSceneName = SceneManager.GetActiveScene().name;

#if INVENTORY_PRO_ADD_ON
            if (dataPersistenceSettings.SaveInventory) LoadInventoryData(gameData);
#endif

            LoadPlayerData(gameData, dataPersistenceSettings, currentSceneName);
            if (dataPersistenceSettings.SaveObjects) LoadObjectsData(gameData, currentSceneName);
        }

        private void LoadPlayerData(GameData gameData, DataPersistence_SO dataPersistenceSettings, string currentSceneName)
        {
            if (dataPersistenceSettings.SavePlayerTransforms)
            {
                // Load Player Position & Rotation & Teleport the Player 
                if (gameData.playerTransforms != null && gameData.playerTransforms.ContainsKey(currentSceneName))
                {
                    SerializablePlayerTransforms serializablePlayerTransforms = gameData.playerTransforms[currentSceneName];
                    Vector3 playerPosition = new Vector3(serializablePlayerTransforms.playerPosition[0], serializablePlayerTransforms.playerPosition[1], serializablePlayerTransforms.playerPosition[2]);
                    Quaternion playerRotation = new Quaternion(serializablePlayerTransforms.playerRotation[0], serializablePlayerTransforms.playerRotation[1], serializablePlayerTransforms.playerRotation[2], serializablePlayerTransforms.playerRotation[3]);

                    playerMovement.TeleportPlayer(playerPosition, playerRotation);

                    // Handle Crouch State
                    if (serializablePlayerTransforms.isCrouching)
                    {
                        playerStates.CurrentState.ExitState();
                        playerStates.CurrentState = playerStates._States.Crouch();
                        playerStates.CurrentState.EnterState();
                    }
                }
            }

            if (dataPersistenceSettings.SavePlayerStats)
            {
                // Load Health & Shield
                playerStats.health = gameData.playerHealth;
                playerStats.shield = gameData.playerShield;
                playerStats.maxHealth = gameData.maxHealth;
                playerStats.maxShield = gameData.maxShield;

                // Load Player Multipliers
                playerMultipliers.damageMultiplier = gameData.damageMultiplier;
                playerMultipliers.healMultiplier = gameData.healMultiplier;

                // Update UI
                uiController.healthDisplayMethod?.Invoke(gameData.playerHealth, gameData.playerShield);
            }

            if (dataPersistenceSettings.SaveExperience)
            {
                // Assign loaded XP
                experienceManager.ResetExperience();
                experienceManager.AddExperience(gameData.totalExperience);
                uiController.UpdateXP(false);
            }

            // Assign loaded Coins
            if (dataPersistenceSettings.SaveCoins)
            {
                coinManager.ResetCoins();
                coinManager.AddCoins(gameData.coins, false);
            }

            // Load FPS Engine�s Base Inventory
            if (dataPersistenceSettings.SaveHotbar)
            {
                weaponController.currentWeapon = gameData.currentWeaponInt;
                LoadHotbarWeaponData(gameData);
            }
        }
#if INVENTORY_PRO_ADD_ON
        private void LoadInventoryData(GameData gameData)
        {
            // If the Player does not use an Inventory, return
            if (InventoryProManager.instance == null) return;

            if (gameData.serializableInventoryData != null && gameData.serializableInventoryData.Length > 0)
            {
                // Manually Generate the Inventory & Hotbar, and then Populate it. If GameDataManager exists in the scene, InventoryProManager is no longer responsible
                // For generating the Inventory & hotbar, but GameDataManager is.
                InventoryProManager.instance._GridGenerator.GenerateInventory();
                InventoryProManager.instance._GridGenerator.GenerateHotbar();
                InventoryProManager.instance._GridGenerator.PopulateInventoryWithSerializableInventoryData(gameData.serializableInventoryData);
            }
            else
            {
                // If no data related to the Inventory has been saved, initialize it with the initial data ( This is generally the first time you start a game )
                InventoryProManager.instance._GridGenerator.GenerateInventory();
                InventoryProManager.instance._GridGenerator.PopulateInventory();
                InventoryProManager.instance._GridGenerator.GenerateHotbar();
            }

            // After Populating the Inventory, calculate the bullets in total.
            InventoryProManager.instance.CalculateTotalBullets(overrideCurrentBullets: true);

            // FAV PINNED MENU
            // We populate the favorites list after the menu is created on Awake by FavItemsMenu and then re-initialize the slots.
            if (!InventoryProManager.instance.UseFavouritesRadialMenu || gameData.favItems == null || gameData.favItems.Length <= 0) return;

            int favSize = gameData.favItems.Length;
            InventoryProManager.instance._FavItemsMenu.favoritesList = new List<FavItemReference>(favSize);

            for (int i = 0; i < favSize; i++)
            {
                var favItem = gameData.favItems[i]; // Get the current favorite item
                Item_SO itemSO = null;

                if (favItem != null)
                {
                    itemSO = ItemRegistry.GetItemByName(favItem.itemName) as Item_SO;
                }

                FavItemReference favItemReference = new FavItemReference(
                    favItem.row,
                    favItem.col,
                    itemSO,
                    favItem.isInventorySlot
                );
                FavItemsMenu favItemsMenu = InventoryProManager.instance._FavItemsMenu;

                // Add to the favorites list (ensure the list has enough capacity)
                if (i < favItemsMenu.favoritesList.Count)
                {
                    favItemsMenu.favoritesList[i] = favItemReference;
                }
                else
                {
                    favItemsMenu.favoritesList.Add(favItemReference);
                }
                favItemsMenu.InventorySlots[i].Initialize(i, favItemReference, favItemsMenu, InventoryProManager.instance, weaponController);
            }

        }
#endif
        private void LoadObjectsData(GameData gameData, string currentSceneName)
        {
#if SAVE_LOAD_ADD_ON
            triggersData = gameData.triggeredObjects;
            // Load triggers
            LoadAndCleanupObjects(
#pragma warning disable CS0618
                FindObjectsOfType<Trigger>(),
#pragma warning restore CS0618
                gameData.triggeredObjects,
                currentSceneName,
                (trigger, data) =>
                {
                    trigger.LoadFields(data);
                }
            );

            interactablesData = gameData.interactablesObjects;
            // Load interactables
            LoadAndCleanupObjects(
#pragma warning disable CS0618
                FindObjectsOfType<Interactable>(),
#pragma warning restore CS0618
                interactablesData,
                currentSceneName,
                (interactable, data) =>
                {
                    interactable.LoadFields(data);
                }
            );

            destructiblesData = gameData.destructibleObjects;
            // Load destructibles
            LoadAndCleanupObjects(
#pragma warning disable CS0618
                FindObjectsOfType<Destructible>(),
#pragma warning restore CS0618
                destructiblesData,
                currentSceneName,
                (destructible, data) =>
                {
                    destructible.LoadFields(data);
                }
            );

            enemyHealthData = gameData.enemyObjects;
            // Load enemies
            LoadAndCleanupObjects(
#pragma warning disable CS0618
                FindObjectsOfType<EnemyHealth>(),
#pragma warning restore CS0618
                enemyHealthData,
                currentSceneName,
                (enemy, data) =>
                {
                    enemy.LoadFields(data);
                }
            );

            customObjectsData = gameData.customObjects;
            // Load Custom Objects
            LoadAndCleanupObjects(
#pragma warning disable CS0618
                FindObjectsOfType<Identifiable>(),
#pragma warning restore CS0618
                customObjectsData,
                currentSceneName,
                (customObject, data) =>
                {
                    customObject.LoadFields(data);
                }
            );

            instantiatedObjects = gameData.instantiatedObjects;
            LoadInstantiatedObjects();
#endif
        }
        private void LoadAndCleanupObjects<TObject, TData>(
        TObject[] sceneObjects,
        Dictionary<string, TData> savedData,
        string currentSceneName,
        Action<TObject, TData> loadAction)
        where TObject : MonoBehaviour
        where TData : class
        {
            if (savedData == null) return;
            var duplicates = sceneObjects
    .GroupBy(o => (o as Identifiable)?.UniqueIDValue)
    .Where(g => g.Count() > 1);

            foreach (var group in duplicates)
            {
                Debug.LogError($"Duplicate ID found: {group.Key}. Count: {group.Count()}");
                foreach (var item in group)
                {
                    Debug.LogError($"Duplicate object: {item.name} in scene {item.gameObject.scene.name}");
                }
            }

            // Create a lookup dictionary for scene objects
            Dictionary<string, TObject> objectLookup = sceneObjects.ToDictionary(o => (o as Identifiable)?.UniqueIDValue);

            // Create a copy of keys to iterate safely
            List<string> keys = savedData.Keys.ToList();

            foreach (var key in keys)
            {
                if (savedData.TryGetValue(key, out TData value))
                {
                    // Check if the data belongs to the current scene
                    // Only those objects that belong to the specific passed scene can be loaded, otherwise they are not meant to exist in the scene
                    if ((value as CustomSaveData)?.SceneName == currentSceneName)
                    {
                        if (objectLookup.TryGetValue(key, out TObject sceneObject))
                        {
                            // Load data into the scene object
                            loadAction(sceneObject, value);
                        }
                        else
                        {
                            // Remove the entry safely
                            savedData.Remove(key);
                        }
                    }
                }
            }
        }

        private void LoadInstantiatedObjects()
        {
            foreach (var pickeable in instantiatedObjects)
            {
                // Normalize the type name 
                string normalizedTypeName = pickeable.Value.Type;

                if (!prefabDictionary.ContainsKey(normalizedTypeName))
                {
                    Debug.LogError($"<color=red>[COWSINS]</color> Key <color=cyan>'{normalizedTypeName}'</color> not found in prefabDictionary. <color=green>Please, assign a prefab for the type in GameDataManager ( This component is attached on DataPersistenceManager )</color>");
                }
                else if (prefabDictionary.TryGetValue(normalizedTypeName, out Identifiable prefab))
                {
                    if (prefab == null)
                    {
                        Debug.LogError($"<color=red>[COWSINS]</color> Prefab is null for type: <color=cyan>{normalizedTypeName}</color>. <color=green>Please, assign a prefab for the type in GameDataManager ( This component is attached on DataPersistenceManager )</color>");
                        continue;
                    }

                    var instantiatedObject = Instantiate(prefab);
                    instantiatedObject.gameObject.SetActive(true);
                    instantiatedObject.GetComponent<Identifiable>().ReplaceUniqueID(pickeable.Key); // Ensure IDs match even after instantiating a new GO. This instantiated object is
                    // meant to be a representation of an object in a different game session, so its ID needs to persist.
                    if (instantiatedObject == null)
                    {
                        Debug.LogError($"<color=red>[COWSINS]</color> Failed to instantiate prefab for type: {normalizedTypeName}");
                        continue;
                    }

                    instantiatedObject.gameObject.SetActive(true);

                    // Teleport the Pickeable if applicable
                    if (pickeable.Value.Position != null)
                    {
                        Vector3 position = pickeable.Value.Position.ToVector3();
                        instantiatedObject.transform.position = position;
                    }
#if SAVE_LOAD_ADD_ON
                    instantiatedObject.LoadFields(pickeable.Value);
#endif
                }
                else
                {
                    Debug.LogWarning($"<color=yellow>[COWSINS]</color>No prefab found for normalized type: {normalizedTypeName}");
                }
            }
        }

        void LoadHotbarWeaponData(GameData gameData)
        {
            if (gameData.serializableHotbarData == null || gameData.serializableHotbarData.Length <= 0) return;

            // Iterate through the Hotbar Data
            for (int i = 0; i < gameData.serializableHotbarData.Length; i++)
            {
                GameData.SerializableInventoryData data = gameData.serializableHotbarData[i];
                string weaponAssetPath = data?.SOPath;

                // If the Weapon does not exist, continue to the next iteration of the loop
                if (string.IsNullOrEmpty(weaponAssetPath)) continue;

                // Look up for the Weapon_SO in the ItemRegistry based on its name
                // ItemRegistry is a collection of Item_SOs that can be retrieved during runtime based on their names.
                Weapon_SO weaponSo = ItemRegistry.GetItemByName(weaponAssetPath) as Weapon_SO;
                if (weaponSo == null)
                {
                    Debug.LogError($"<color=red>[COWSINS]</color> Weapon asset '{weaponAssetPath}' not found.");
                    continue;
                }

                // WeaponSO is not null, we can safely instantiate the weapon
                weaponController.InstantiateWeapon(weaponSo, i, data.bulletsLeftInMagazine, data.totalBullets);

                // Handle Attachments
                var attachmentPaths = new[] { data.barrel, data.scope, data.stock, data.grip, data.magazine, data.flashlight, data.laser };
                var attachments = new AttachmentIdentifier_SO[attachmentPaths.Length];

                // Retrieve all attachments once
                for (int j = 0; j < attachmentPaths.Length; j++)
                {
                    attachments[j] = ItemRegistry.GetItemByName(attachmentPaths[j]) as AttachmentIdentifier_SO;
                }

                // Assign attachments to weapon
                foreach (var attachment in attachments)
                {
                    (Attachment atc, int id) = CowsinsUtilities.GetAttachmentID(attachment, weaponController.inventory[i]);
                    weaponController.AssignAttachmentToWeapon(atc, id, i);
                }

#if INVENTORY_PRO_ADD_ON
                if (InventoryProManager.instance == null) continue;
                // Assign to inventory slot from Inventory Pro Add-On
                InventoryProManager.instance.HotbarSlots[i].slotData = new SlotData(
                    weaponSo, 1, data.bulletsLeftInMagazine, data.totalBullets,
                    attachments[0], attachments[1], attachments[2], attachments[3], attachments[4], attachments[5], attachments[6]);

                // Update Inventory Hotbar Slots UI
                InventoryProManager.instance.HotbarSlots[i].UpdateSlotGraphics();
#endif
            }
        }
        #endregion

        #region Drop

        public void OnDropHotbar(Pickeable pick)
        {
            if (instance.instantiatedObjects == null)
                instance.instantiatedObjects = new Dictionary<string, CustomSaveData>();
#if SAVE_LOAD_ADD_ON
            pick.SaveInstance();
#endif
        }

        #endregion
    }
}