using cowsins.SaveLoad;
using UnityEngine;
using System.Linq; 
using System.Collections.Generic;

namespace cowsins.Inventory
{
    /// <summary>
    /// GridGenerator Handles Generation, Population and Management of all grids in Inventory Pro Manager Add-On, including Inventory + Chests. 
    /// </summary>
    public class GridGenerator : MonoBehaviour
    {
        [SerializeField, Tooltip("Reference to the InventoryProManager (parent of GridGenerator)")] private InventoryProManager inventoryManager;
        [SerializeField, Tooltip("Stores Initial Slot Data Information & Customization about the Player Inventory.")] private InventoryGridData_SO inventoryGridData;
        [SerializeField, Tooltip("Transform that stores all Hotbar Inventory Slots.")] private Transform hotbarParent;
        [SerializeField, Tooltip("Transform that stores all Inventory Slots from the Inventory.")] private Transform buttonParent;
        [SerializeField, Tooltip("Transform that stores all Chest Inventory Slots. Chests Grid are created dynamically.")] private Transform chestParent;
        [SerializeField, Tooltip("Horizontal Gap/Spacing between Inventory Slots")] private float gapX = 120f;
        [SerializeField, Tooltip("Vertical Gap/Spacing between Inventory Slots")] private float gapY = 120f;
        [SerializeField, Tooltip("Hotbar Spacing from the Inventory")] private Padding hotbarPadding;

        // REFERENCES 
        private WeaponController weaponController;
        private PlayerMultipliers playerMultipliers;
        private InventorySlot[,] inventorySlots;

        // CONTROLLER NAVIGATION
        [HideInInspector] public int selectedControllerSlotRow = 0;
        [HideInInspector] public int selectedControllerSlotColumn = 0;

        // GETTERS
        public Transform HotbarParent => hotbarParent;
        public Transform ChestParent => chestParent;
        public InventorySlot[,] InventorySlots => inventorySlots;
        private InventorySlot[] hotbarSlots;
        public InventorySlot[] HotbarSlots => hotbarSlots;
        private InventorySlot[,] chestSlots;
        public InventorySlot[,] ChestSlots => chestSlots;
        private Chest currentChest;
        public Chest CachedChest => currentChest;
        public bool isChestOpen => currentChest != null;
        public InventoryGridData_SO _InventoryGridData_SO => inventoryGridData;
        public int Rows => inventoryGridData.rows;
        public int Columns => inventoryGridData.columns;
        public float GapX => gapX;
        public float GapY => gapY;
        public Padding HotbarPadding => hotbarPadding;

        /// <summary>
        /// Generates Player´s Inventory Grid. This leaves the Inventory Blank, as it does not populate.
        /// </summary>
        public void GenerateInventory()
        {
            inventorySlots = new InventorySlot[Rows, Columns];
            GenerateGrid(SlotType.Inventory, Rows, Columns, inventoryGridData.inventoryPadding, buttonParent, inventorySlots, inventoryGridData.buttonPrefab);
            inventoryManager.Events.onInventoryGenerated?.Invoke();
        }
        /// <summary>
        /// Generates Player´s Hotbar & Populates it with Initial Weapons Data from WeaponController.
        /// </summary>
        public void GenerateHotbar()
        {
            // Hotbar Slots are the same size as the Inventory Size from Weapon Controller ( Size of the collection of weapons that the player can carry simultanously )
            hotbarSlots = new InventorySlot[weaponController.inventorySize];

            Vector3 offset = new Vector3(hotbarPadding.horizontal, hotbarPadding.vertical, 0);

            // Instantiate, Populate & Initialize each Inventory Slot individually.
            for (int i = 0; i < weaponController.inventorySize; i++)
            {
                InventorySlot slot = GenerateSlot(SlotType.Hotbar, 0, i, offset, hotbarParent, null, inventoryGridData.buttonPrefab);

                Weapon_SO weapon = i >= 0 && i < weaponController.initialWeapons.Length ? weaponController.initialWeapons[i] : null;

                SlotData slotData = weapon != null ? new SlotData(weapon, 1) : new SlotData();

                if (slotData?.item is Weapon_SO)
                {
                    InitializeInitialWeapon(slotData, weapon);
                }
                slot.Initialize(-1, i, slotData, weaponController, inventoryManager);
            }
            inventoryManager.Events.onHotbarGenerated?.Invoke();
        }

        /// <summary>
        /// Opens a chest given a Chest Reference and its SlotData. Chests are created dynamically, meaning that each time a Chest is opened, a grid is created. When a chest is closed,
        /// the grid is destroyed ( the chest data is stored in the respective Chest, though ).
        /// This is a useful approach to ensure different sizes from different chests.
        /// </summary>
        /// <param name="chest">Chest to open</param>
        /// <param name="chestSlotData">Slot Data from the Chest. It is very important to pass the current chest data, not the initial data.</param>
        /// <param name="chestRows">Rows size.</param>
        /// <param name="chestColumns">Columns Size</param>
        public void OpenChest(Chest chest, SlotData[] chestSlotData, int chestRows, int chestColumns)
        {
            // Enables Chest UI
            chestParent.gameObject.SetActive(true);
            currentChest = chest;
            chestSlots = new InventorySlot[chestRows, chestColumns];

            // Generate Chest Grid
            GenerateGrid(SlotType.Chest, chestRows, chestColumns, chest.InitialInventoryGridData.inventoryPadding, chestParent, chestSlots, chest.InitialInventoryGridData.buttonPrefab);

            // Populate Chest Grid
            for (int row = 0; row < chestRows; row++)
            {
                for (int col = 0; col < chestColumns; col++)
                {
                    InventorySlot slot = chestSlots[row, col];

                    bool hasSlotData = chestSlotData.Length > 0 && chestSlotData.Length > row * chestColumns + col && chestSlotData[row * chestColumns + col] != null;

                    SlotData data = hasSlotData ? chestSlotData[row * chestColumns + col] : new SlotData();
                    SlotData slotData = chestSlots[row, col].anchorSlot == null ? data : new SlotData();
                    chestSlots[row, col].Initialize(row, col, slotData, weaponController, inventoryManager);
                }
            }

            // Now, we need to update the size of each item populated in the Chest Slots. This is only called for Tetris Inventories.
            UpdateSlotsSize(chestSlots);

            inventoryManager.Events.onOpenChest?.Invoke();
        }

        /// <summary>
        /// Closes Chest UI & Destroys Chest Grid. Chest Data is stored into the appropriate chest before closing.
        /// </summary>
        public void CloseChest()
        {
            chestParent.gameObject.SetActive(false);

            if (chestSlots == null)
            {
                currentChest = null;
                return;
            }

            List<SlotData> chestSlotsData = new List<SlotData>();

            foreach (InventorySlot slot in chestSlots)
            {
                chestSlotsData.Add(slot.slotData);
                Destroy(slot.gameObject);
            }

            // Ensure Chest Data is saved into the appropriate Chest.
            // This information is passed to the GameDataManager in case we pretend to save the game.
            chestSlots = new InventorySlot[0, 0];
            currentChest?.SetChestData(chestSlotsData.ToArray());
#if SAVE_LOAD_ADD_ON
            currentChest?.StoreData();
#endif
            currentChest = null;
            
            inventoryManager.Events.onCloseChest?.Invoke();
        }

        /// <summary>
        /// Generates a Grid of Ivnentory Slots. Helper method to create Player Inventory & Chests.
        /// </summary>
        /// <param name="slotType">Type of Inventory to Create.</param>
        /// <param name="_rows">Rows Size.</param>
        /// <param name="_cols">Columns Size.</param>
        /// <param name="padding">Used to calculate Slot offsets</param>
        /// <param name="instantiationParent">InventorySlots will be children of this Transform</param>
        /// <param name="slotsArray">InventorySlot[,] to store the Slots at.</param>
        /// <param name="buttonPrefab">InventorySlot Prefab to Instantiate</param>
        private void GenerateGrid(SlotType slotType, int _rows, int _cols, Padding padding, Transform instantiationParent, InventorySlot[,] slotsArray, GameObject buttonPrefab)
        {
            // Starting from the top-left
            Vector3 offset = new Vector3(padding.horizontal, padding.vertical, 0);

            float totalWidth = _cols * gapX + (padding.horizontal * 2);
            float totalHeight = _rows * gapY + (padding.vertical * 2);

            RectTransform parentRect = instantiationParent.GetComponent<RectTransform>();
            if (parentRect != null)
                parentRect.sizeDelta = new Vector2(totalWidth, totalHeight);

            // Generate each slot individually.
            for (int row = 0; row < _rows; row++)
            {
                for (int col = 0; col < _cols; col++)
                {
                    GenerateSlot(slotType, row, col, offset, instantiationParent, slotsArray, buttonPrefab);
                }
            }
        }

        /// <summary>
        /// Instantiates an Inventory Slot, Initializes it and places it inside the proper array.
        /// </summar
        private InventorySlot GenerateSlot(SlotType slotType, int row, int col, Vector3 offset, Transform instantiationParent, InventorySlot[,] slotsArray, GameObject buttonPrefab)
        {
            GameObject button = Instantiate(buttonPrefab, instantiationParent);
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            bool isHotbar = slotType == SlotType.Hotbar;
            // Top-left pivot
            rectTransform.pivot = isHotbar ? new Vector2(0,0) : new Vector2(0, 1);

            rectTransform.localPosition = new Vector3(col * gapX, -row * gapY - offset.y * 2, 0) + offset;
            button.name = $"Button[{row},{col}]";

            InventorySlot slot = button.GetComponent<InventorySlot>();
            if (isHotbar)
            {
                hotbarSlots[col] = slot;
                slot.slotType = SlotType.Hotbar;
            }
            else
            {
                slotsArray[row, col] = slot;
                slot.slotType = slotType;
            }

            return slot;
        }

        // Used for Hotbar Slots, handles attachments & bullets for Initial Weapons in the Hotbar Inventory.
        private void InitializeInitialWeapon(SlotData data, Weapon_SO weapon)
        {
            DefaultAttachment defaultAttachments = weapon.weaponObject.defaultAttachments;
            data.barrel = defaultAttachments.defaultBarrel?.attachmentIdentifier;
            data.scope = defaultAttachments.defaultScope?.attachmentIdentifier;
            data.stock = defaultAttachments.defaultStock?.attachmentIdentifier;
            data.grip = defaultAttachments.defaultGrip?.attachmentIdentifier;
            data.magazine = defaultAttachments.defaultMagazine?.attachmentIdentifier;
            data.flashlight = defaultAttachments.defaultFlashlight?.attachmentIdentifier;
            data.laser = defaultAttachments.defaultLaser?.attachmentIdentifier;

            data.bulletsLeftInMagazine = weapon.magazineSize;
            data.totalBullets = weapon.limitedMagazines ? weapon.magazineSize * weapon.totalMagazines : weapon.magazineSize;
        }

        /// <summary>
        /// Returns true if the given slot fits in the target Slot based on its size. This is used for Tetris Inventories.
        /// </summary>
        private bool ItemFits(InventorySlot slot, Vector2Int itemSize, int row, int col, bool isInventorySlot)
        {
            InventorySlot[,] inventorySlotsToCheck = isInventorySlot ? inventorySlots : chestSlots;
            if (!isInventorySlot && CachedChest?.InitialInventoryGridData == null) return false;
            int _rows = isInventorySlot ? Rows : CachedChest!.InitialInventoryGridData!.rows;
            int _cols = isInventorySlot ? Rows : CachedChest!.InitialInventoryGridData!.columns;

            // Ensure that the item does not exceed the grid dimensions.
            if (!IsSlotWithinBounds(row, col, itemSize, inventorySlotsToCheck))
                return false;

            for (int i = 0; i < itemSize.y; i++)
            {
                for (int j = 0; j < itemSize.x; j++)
                {
                    InventorySlot slotToCheck = inventorySlotsToCheck[row + i, col + j];

                    // Check if any slot is occupied
                    if (slotToCheck.anchorSlot != null && slotToCheck.GetAnchorSlot().HasValidItem)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Populate Player Inventory with given Data, generally used by GameDataManager with Loaded Data.
        /// </summary>
        /// <param name="loadedData"></param>
        public void PopulateInventoryWithSerializableInventoryData(GameData.SerializableInventoryData[] loadedData)
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    InventorySlot slot = inventorySlots[row, col];
                    var loadedDataEntry = loadedData[row * Columns + col];

                    // Create a temporal SlotData
                    // Since Item_SOs are not serializable, we store their name. In order to properly load them as Item_SOs ( AttachmentIdentifier_SOs in this case )
                    // We´ll gather the item from the ItemRegistry based on their name.
                    SlotData data = new SlotData
                    {
                        amount = loadedDataEntry.amount,
                        bulletsLeftInMagazine = loadedDataEntry.bulletsLeftInMagazine,
                        totalBullets = loadedDataEntry.totalBullets,
                        item = ItemRegistry.GetItemByName(loadedDataEntry.SOPath),
                        isOriented = loadedDataEntry.isOriented,
                        barrel = ItemRegistry.GetItemByName(loadedDataEntry.barrel) as AttachmentIdentifier_SO,
                        scope = ItemRegistry.GetItemByName(loadedDataEntry.scope) as AttachmentIdentifier_SO,
                        stock = ItemRegistry.GetItemByName(loadedDataEntry.stock) as AttachmentIdentifier_SO,
                        grip = ItemRegistry.GetItemByName(loadedDataEntry.grip) as AttachmentIdentifier_SO,
                        magazine = ItemRegistry.GetItemByName(loadedDataEntry.magazine) as AttachmentIdentifier_SO,
                        flashlight = ItemRegistry.GetItemByName(loadedDataEntry.flashlight) as AttachmentIdentifier_SO,
                        laser = ItemRegistry.GetItemByName(loadedDataEntry.laser) as AttachmentIdentifier_SO,
                    };

                    // Initialize the slot and update the weight of the carried Items.
                    InitializeSlot(data, row, col, slot, false);
                    UpdateCurrentWeight(data.item, data.amount);
                }
            }

            // After populating the entire Inventory, Update the Size of the Slots for Tetris Inventor.
            UpdateSlotsSize(inventorySlots);
            UpdateHotbarSlotsVisuals();
        }

        /// <summary>
        /// Populates pLayer Inventory with Initial Data from the Inventory Grid Data. This is called when a game is started for the first time or if GameDataManager 
        /// doe snopt exist in the scene.
        /// </summary>
        public void PopulateInventory()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    InventorySlot slot = inventorySlots[row, col];
                    SlotData data = new SlotData();
                    if (inventoryGridData.inventoryInitialData.Length > row * Columns + col)
                    {
                        SlotData newData = inventoryGridData.inventoryInitialData[row * Columns + col];
                        data.item = newData.item;
                        data.amount = newData.amount;
                        data.bulletsLeftInMagazine = newData.bulletsLeftInMagazine;
                        data.totalBullets = newData.totalBullets;
                        data.barrel = newData.barrel;
                        data.scope = newData.scope;
                        data.stock = newData.stock;
                        data.grip = newData.grip;
                        data.magazine = newData.magazine;
                        data.flashlight = newData.flashlight;
                        data.laser = newData.laser;
                    }

                    InitializeSlot(data, row, col, slot, true);
                    UpdateCurrentWeight(data.item, data.amount);
                }
            }

            // After populating the entire Inventory, Update the Size of the Slots for Tetris Inventor.
            UpdateSlotsSize(inventorySlots);
        }

        private void InitializeSlot(SlotData data, int row, int col, InventorySlot slot, bool setDefaultAttachments)
        {
            if (setDefaultAttachments && data.item is Weapon_SO)
            {
                Weapon_SO weapon = (Weapon_SO)data.item;
                InitializeInitialWeapon(data, weapon);
            }

            SlotData slotData = inventorySlots[row, col].anchorSlot == null ? data : new SlotData();
            inventorySlots[row, col].Initialize(row, col, slotData, weaponController, inventoryManager);

            if (inventoryManager._InventoryStyle != InventoryProManager.InventoryStyle.Tetris && slotData?.item != null)
            {
                slot.anchorSlot = slot;
            }
        }

        /// <summary>
        /// Clears the entire Area ( From the anchor as the pivot / Origin ) that a slot occupies based on its item size 
        /// </summary>
        /// <param name="slot"></param>
        public void ClearSlotArea(InventorySlot slot)
        {
            InventorySlot[,] slots = slot.IsChestSlot ? chestSlots : inventorySlots;
            InventorySlot anchor = slot.GetAnchorSlot();

            Vector2Int itemSize = anchor.GetItemSize();

            int startRow = anchor.row;
            int startCol = anchor.col;
            int biggerValue = Mathf.Max(itemSize.x, itemSize.y); 

            for (int i = 0; i < biggerValue; i++)
            {
                int rowIndex = startRow + i;
                if (rowIndex >= slots.GetLength(0)) continue;
                for (int j = 0; j < biggerValue; j++)
                {
                    int colIndex = startCol + j;
                    if (colIndex >= slots.GetLength(1)) continue;
                    if (slots[rowIndex, colIndex].anchorSlot == anchor)
                    {
                        slots[rowIndex, colIndex].ClearSlot();
                    }
                }
            }
        }


        private void UpdateSlotsSize(InventorySlot[,] slotsArray)
        {
            if (inventoryManager._InventoryStyle != InventoryProManager.InventoryStyle.Tetris) return;

            bool isInventorySlot = slotsArray[0, 0].IsInventorySlot;
            int _rows = isInventorySlot ? Rows : CachedChest.InitialInventoryGridData.rows;
            int _cols = isInventorySlot ? Columns : CachedChest.InitialInventoryGridData.columns;

            foreach (InventorySlot slot in slotsArray)
            {
                if (slot.HasValidItem)
                {
                    // Get item size
                    Vector2Int size = slot.GetItemSize();
                    int itemWidth = size.x;
                    int itemHeight = size.y;
                    int startRow = slot.row;
                    int startCol = slot.col;

                    if (!ItemFits(slot, size, startRow, startCol, isInventorySlot))
                    {
                        Debug.LogError($"<color=red>[COWSINS]</color> Item at Row: <b><color=yellow>{startRow}</color></b> Col: <b><color=yellow>{startCol} ({slot.slotData?.item?._name})</color></b> " +
                                        $"in <b><color=yellow>{inventoryGridData.name}</color></b> does not fit in the Inventory & overlaps with other items or is out of bounds. " +
                                            "<b><color=green>Please, open the Inventory Designer & relocate or delete the item</color></b> to fix this error.", this);
                        ClearSlotArea(slot);
                        continue;
                    }
                    // Loop through the area that the item occupies
                    for (int i = 0; i < itemHeight; i++)
                    {
                        for (int j = 0; j < itemWidth; j++)
                        {
                            // Check for bounds
                            int targetRow = startRow + i;
                            int targetCol = startCol + j;

                            if (targetRow < 0 || targetRow >= _rows || targetCol < 0 || targetCol >= _cols)
                            {
                                Debug.LogError($"<color=red>[COWSINS]</color> Item at Row: <b><color=yellow>{startRow}</color></b> Col: <b><color=yellow>{startCol} ({slot.slotData?.item?._name})</color></b> " +
                                        $"in <b><color=yellow>{inventoryGridData.name}</color></b> does not fit in the Inventory & overlaps with other items or is out of bounds. " +
                                            "<b><color=green>Please, open the Inventory Designer & relocate or delete the item</color></b> to fix this error.", this);
                                ClearSlotArea(slot);
                                return;
                            }
                            else if (slotsArray[targetRow, targetCol].anchorSlot == null)
                            {
                                // Assign the occupiedByAnchor if the slot is valid
                                slotsArray[targetRow, targetCol].anchorSlot = slot;
                            }
                        }
                    }
                }
                slot.UpdateSlotGraphics();
            }
        }
        private void UpdateHotbarSlotsVisuals()
        {
            // Iterate through each slot in the inventory
            // The hotbar slots size is the same size as inventorySize from WeaponController
            for (int i = 0; i < weaponController.inventorySize; i++)
            {
                var currentWeapon = weaponController.inventory[i];

                if (currentWeapon == null) continue;

                // Create and assign new SlotData for the hotbar slot
                hotbarSlots[i].slotData = new SlotData
                {
                    item = currentWeapon.weapon,
                    amount = 1,
                    bulletsLeftInMagazine = currentWeapon.bulletsLeftInMagazine,
                    totalBullets = currentWeapon.totalBullets
                };

                // Update the slot visuals to match the new data
                hotbarSlots[i].UpdateSlotGraphics();
            }
        }

        /// <summary>
        /// Returns all bullets found in the Inventory
        /// </summary>
        /// <returns>Returns Bullet Count</returns>
        public int GatherBulletsInInventory()
        {
            int amount = 0;

            for (int i = 0; i < Columns * Rows; i++)
            {
                (int iRow, int iCol) = GetRowAndColumnOutOfIndex(i);
                InventorySlot inventorySlot = inventorySlots[iRow, iCol].GetComponent<InventorySlot>();
                if (inventorySlot.slotData.item is BulletsItem_SO bulletsSO)
                {
                    amount += inventorySlot.slotData.amount;
                }
            }

            return amount;
        }

        /// <summary>
        /// Reduce the specified amount of bullets from the Inventory.
        /// </summary>
        /// <param name="amount">Amount of bullets to remove.</param>
        public void ReduceBulletsInInventory(int amount)
        {
            int remainingToReduce = amount; // Keep track of how much we still need to reduce

            // Loop through the slots
            for (int i = 0; i < Columns * Rows; i++)
            {
                (int iRow, int iCol) = GetRowAndColumnOutOfIndex(i);
                InventorySlot inventorySlot = inventorySlots[iRow, iCol].GetComponent<InventorySlot>();

                // Check if the item in the slot is a bullet item
                if (inventorySlot.slotData.item is BulletsItem_SO bulletsSO)
                {
                    int bulletsInSlot = inventorySlot.slotData.amount;

                    // Determine how much to reduce in the current slot
                    int amountToReduceFromThisSlot = Mathf.Min(remainingToReduce, bulletsInSlot);

                    // Reduce the bullets in the current slot
                    inventorySlot.slotData.amount -= amountToReduceFromThisSlot;
                    remainingToReduce -= amountToReduceFromThisSlot;

                    UpdateCurrentWeight(bulletsSO, -amountToReduceFromThisSlot);

                    Vector2Int originalSize = inventorySlot.GetItemSize();

                    // Update the slot graphics and inventory management
                    for (int u = 0; u < originalSize.x; u++)
                    {
                        for (int v = 0; v < originalSize.y; v++)
                        {
                            if (inventorySlot.slotData.amount <= 0)
                            {
                                inventorySlots[inventorySlot.row + u, inventorySlot.col + v].anchorSlot = null;
                            }
                            inventorySlots[inventorySlot.row + u, inventorySlot.col + v].UpdateSlotGraphics();
                        }
                    }

                    // If no more bullets need to be reduced, break the loop
                    if (remainingToReduce <= 0)
                    {
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Adds the Given Item to the Inventory. Returns (bool success, int amount): success is true if the entire amount of the item has been successfully added to the Inventory.
        /// If success is true, amount equals 0. If success is false, amount will return the amount of Items that were not added to the Inventory.
        /// </summary>
        /// <param name="item_SO">Item to add.</param>
        /// <param name="amount">Amount to add.</param>
        /// <returns>Returns (bool success, int amount): success is true if the entire amount of the item has been successfully added to the Inventory.
        /// If success is true, amount equals 0. If success is false, amount will return the amount of Items that were not added to the Inventory.</returns>
        public (bool, int) AddItemToInventory(Item_SO item_SO, int amount)
        {
            // If Item is null, return.
            if (item_SO == null) return (false, amount);

            if (amount <= 0) amount = 1;
            int availableAmount = amount;
            float itemWeight = item_SO.Weight.Weight;

            // First, try to add to existing stacks
            if (item_SO.maxStack > 1)
            {
                for (int i = 0; i < Columns * Rows; i++)
                {
                    (int iRow, int iCol) = GetRowAndColumnOutOfIndex(i);
                    InventorySlot inventorySlot = InventorySlots[iRow, iCol];

                    if (inventorySlot.HasValidItem && inventorySlot.slotData.item == item_SO)
                    {
                        int amountInSlot = inventorySlot.slotData.amount;
                        int maxAmountPerSlot = item_SO.maxStack;

                        if (amountInSlot < maxAmountPerSlot)
                        {
                            int availableSpace = maxAmountPerSlot - amountInSlot;
                            int amountToAdd = Mathf.Min(availableAmount, availableSpace);

                            inventorySlot.slotData.amount += amountToAdd;
                            availableAmount -= amountToAdd;

                            UpdateCurrentWeight(item_SO, amountToAdd);
                            Vector2Int size = inventorySlot.GetItemSize();
                            for (int u = 0; u < size.x; u++)
                            {
                                for (int v = 0; v < size.y; v++)
                                {
                                    inventorySlots[inventorySlot.row + u, inventorySlot.col + v].UpdateSlotGraphics();
                                }
                            }

                            if (availableAmount <= 0)
                                return (true, 0);
                        }
                    }
                }
            }
            // If there are still items to add, find empty slots and add new stacks
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    InventorySlot inventorySlot = inventorySlots[i, j];
                    Vector2Int size = inventoryManager._InventoryStyle == InventoryProManager.InventoryStyle.Grid ? new Vector2Int(1, 1) : item_SO.itemSize;
                    
                    if (inventorySlot.slotData.item == null && inventorySlot.GetAnchorSlot()?.slotData?.item == null && ItemFits(inventorySlot, size, inventorySlot.row, inventorySlot.col, inventorySlot.IsInventorySlot))
                    {
                        int amountToAdd = Mathf.Min(availableAmount, item_SO.maxStack);

                        inventorySlot.slotData = new SlotData
                        {
                            item = item_SO,
                            amount = amountToAdd
                        };

                        if (item_SO is Weapon_SO weapon_SO)
                        {
                            inventorySlot.slotData.bulletsLeftInMagazine = weapon_SO.magazineSize;
                            inventorySlot.slotData.totalBullets = weapon_SO.magazineSize;
                        }

                        for (int u = 0; u < size.y; u++)
                        {
                            for (int v = 0; v < size.x; v++)
                            {
                                inventorySlots[inventorySlot.row + u, inventorySlot.col + v].anchorSlot = inventorySlot;
                                inventorySlots[inventorySlot.row + u, inventorySlot.col + v].UpdateSlotGraphics();
                            }
                        }

                        UpdateCurrentWeight(item_SO, amountToAdd);
                        availableAmount -= amountToAdd;

                        if (availableAmount <= 0)
                            return (true, 0);
                    }
                }
            }

            return (availableAmount < amount, availableAmount);
        }

        /// <summary>
        /// Adds the Given Weapon to the Inventory. Returns true if the weapon has been successfully added to the Inventory.
        /// </summary>
        /// <param name="weapon_SO">Item to add.</param>
        /// <returns>Returns Returns true if the weapon has been successfully added to the Inventory.</returns>
        public bool AddWeaponToInventory(Weapon_SO weapon_SO, int bulletsLeftInmagazine, int totalBullets)
        {
            // If Item is null, return.
            if (weapon_SO == null ) return false;

            float itemWeight = weapon_SO.Weight.Weight;

            // Find empty slots 
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    InventorySlot inventorySlot = inventorySlots[i, j];
                    Vector2Int size = inventoryManager._InventoryStyle == InventoryProManager.InventoryStyle.Grid ? new Vector2Int(1, 1) : weapon_SO.itemSize;

                    if (inventorySlot.slotData.item == null && inventorySlot.GetAnchorSlot()?.slotData?.item == null && ItemFits(inventorySlot, size, inventorySlot.row, inventorySlot.col, inventorySlot.IsInventorySlot))
                    {
                        int amountToAdd = 1;

                        inventorySlot.slotData = new SlotData
                        {
                            item = weapon_SO,
                            amount = amountToAdd,
                            bulletsLeftInMagazine = bulletsLeftInmagazine,
                            totalBullets = totalBullets
                        };

                        for (int u = 0; u < size.y; u++)
                        {
                            for (int v = 0; v < size.x; v++)
                            {
                                inventorySlots[inventorySlot.row + u, inventorySlot.col + v].anchorSlot = inventorySlot;
                                inventorySlots[inventorySlot.row + u, inventorySlot.col + v].UpdateSlotGraphics();
                            }
                        }

                        UpdateCurrentWeight(weapon_SO, amountToAdd);
                        return true;
                    }
                }
            }

            // No free space found
            return false;
        }

        public void UpdateCurrentWeight(Item_SO item, int amountToAdd = 1)
        {
            if (item == null) return;
            if (G.PlayerStats is null)
            {
                Debug.Log("Weight system is null");
                return;
            }
            if (amountToAdd > 0)
            {
                G.PlayerStats.WeightSystem.CanPickUp(item.Weight, amountToAdd);
            }
            else
            {
                G.PlayerStats.WeightSystem.RemoveItems(item.Weight, amountToAdd);
            }
        }

        /// <summary>
        /// Returns whether the Player has enough ingredients in the Inventory to afford crafting the given recipe and the given amount.
        /// </summary>
        /// <param name="recipe">Recipe to Craft.</param>
        /// <param name="craftAmount">Amount of Items ( Recipe Result ) to Craft.</param>
        public bool HasEnoughIngredients(Recipe_SO recipe, int craftAmount)
        {
            Dictionary<Item_SO, int> inventoryDict = new Dictionary<Item_SO, int>();

            int rows = inventorySlots.GetLength(0);
            int cols = inventorySlots.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Item_SO item = inventorySlots[i, j].slotData?.item;
                    if (item != null)
                    {
                        if (!inventoryDict.ContainsKey(item))
                            inventoryDict[item] = 0;

                        inventoryDict[item] += inventorySlots[i, j].slotData.amount;
                    }
                }
            }

            // Convert Recipe Ingredients to Dictionary for easier lookup
            Dictionary<Item_SO, int> recipeDict = recipe.ingredients
                .GroupBy(ing => ing.item)
                .ToDictionary(g => g.Key, g => g.Sum(ing => ing.quantity) * craftAmount); 

            // Check if Inventory has enough of each required item
            foreach (var req in recipeDict)
            {
                if (!inventoryDict.TryGetValue(req.Key, out int count) || count < req.Value)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the player has at least the required amount of the specified item in their inventory.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <param name="requiredAmount">The amount needed.</param>
        public bool HasEnoughOfItem(Item_SO item, int requiredAmount)
        {
            if (item == null || requiredAmount <= 0)
                return false;

            int totalFound = 0;

            int rows = inventorySlots.GetLength(0);
            int cols = inventorySlots.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var slotData = inventorySlots[i, j].slotData;
                    if (slotData?.item == item)
                    {
                        totalFound += slotData.amount;
                        if (totalFound >= requiredAmount)
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the total amount of the specified item found in the player's inventory.
        /// </summary>
        public int GetAmountOfItem(Item_SO item)
        {
            if (item == null)
                return 0;

            int totalFound = 0;

            int rows = inventorySlots.GetLength(0);
            int cols = inventorySlots.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var slotData = inventorySlots[i, j].slotData;
                    if (slotData?.item == item)
                    {
                        totalFound += slotData.amount;
                    }
                }
            }

            return totalFound;
        }

        /// <summary>
        /// Remove all Items (Ingredients) required by the recipe from the Inventory
        /// </summary>
        /// <param name="recipe">Recipe to Craft.</param>
        /// <param name="craftAmount">Amount of Items ( Recipe Result ) to Craft.</param>
        public bool PayIngredients(Recipe_SO recipe, int craftAmount)
        {
            // Convert Recipe Ingredients to Dictionary for easier lookup
            Dictionary<Item_SO, int> requiredIngredients = recipe.ingredients
                .GroupBy(ing => ing.item)
                .ToDictionary(g => g.Key, g => g.Sum(ing => ing.quantity) * craftAmount);

            int rows = inventorySlots.GetLength(0);
            int cols = inventorySlots.GetLength(1);

            // Loop through required ingredients and remove them from inventory
            foreach (var req in requiredIngredients)
            {
                Item_SO requiredItem = req.Key;
                // Total amount we need to remove
                int remainingAmountToRemove = req.Value;

                for (int i = 0; i < rows && remainingAmountToRemove > 0; i++)
                {
                    for (int j = 0; j < cols && remainingAmountToRemove > 0; j++)
                    {
                        var slot = inventorySlots[i, j];

                        // Found matching item
                        if (slot.slotData?.item == requiredItem) 
                        {
                            int slotAmount = slot.slotData.amount;

                            if (slotAmount >= remainingAmountToRemove)
                            {
                                // We can complete the requirement from this slot alone
                                slot.slotData.amount -= remainingAmountToRemove;
                                UpdateCurrentWeight(requiredItem, -remainingAmountToRemove);
                                // Clear slot if empty
                                if (slot.slotData.amount == 0)
                                {
                                    if (inventoryManager.UseFavouritesRadialMenu) inventoryManager._FavItemsMenu.UnpinFromFavMenu(slot);
                                    slot.slotData.item = null;
                                    slot.ClearSlot();
                                }
                                else slot.UpdateSlotGraphics();
                                // Requirement met already
                                remainingAmountToRemove = 0;
                            }
                            else
                            {
                                // Remove all from this slot and move to the next one
                                remainingAmountToRemove -= slotAmount;
                                UpdateCurrentWeight(requiredItem, -slotAmount);

                                if (inventoryManager.UseFavouritesRadialMenu) inventoryManager._FavItemsMenu.UnpinFromFavMenu(slot);
                                slot.slotData.amount = 0;
                                slot.slotData.item = null; 
                                slot.ClearSlot();
                            }
                        }
                    }
                }

                // ERROR CHECK: Could not remove enough? 
                if (remainingAmountToRemove > 0)
                {
                    return false;
                }

            }

            return true;
        }

        /// <summary>
        /// Returns true if the slot fits within the given Inventory bounds.
        /// </summary>
        public bool IsSlotWithinBounds(int row, int col, Vector2Int itemSize, InventorySlot[,] inventorySlots)
        {
            if (row < 0 || row + itemSize.y > inventorySlots.GetLength(0) ||
                col < 0 || col + itemSize.x > inventorySlots.GetLength(1))
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Returns true if the slot fits within the given Inventory bounds.
        /// </summary>
        public bool IsSlotWithinBounds(int targetRow, int targetCol, InventorySlot[,] inventorySlots)
        {
            return targetRow >= 0 && targetRow < inventorySlots.GetLength(0) &&
                    targetCol >= 0 && targetCol < inventorySlots.GetLength(1);
        }
        /// <summary>
        /// Returns whether the current chest is full or not.
        /// </summary>
        public bool IsChestFull()
        {
            if (chestSlots == null) return true;
            foreach (var item in chestSlots)
            {
                if (item.slotData == null || item.slotData.item == null) return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the Player Inventory is full or not.
        /// </summary>
        public bool IsInventoryFull()
        {
            if (inventorySlots == null) return true;
            foreach (var slot in inventorySlots)
            {
                if (slot.slotData == null || slot.slotData.item == null) return false;
            }

            return false;
        }

        // Converts an index into a tuple representing the corresponding row and column in a grid.
        private (int row, int col) GetRowAndColumnOutOfIndex(int index)
        {
            int row = index / Columns;
            int col = index % Columns;
            return (row, col);
        }

        /// <summary>
        /// Initializes Basic References
        /// </summary>
        public void Initialize(WeaponController controller)
        {
            weaponController = controller;
            playerMultipliers = weaponController.GetComponent<PlayerMultipliers>();
        }
    }
}