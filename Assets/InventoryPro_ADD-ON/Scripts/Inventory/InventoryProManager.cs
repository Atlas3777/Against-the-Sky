using cowsins.SaveLoad;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace cowsins.Inventory
{
    /// <summary>
    /// The InventoryProManager is the most essential part of the Inventory System. It manages Inventory Slots, Highlighted Slots, Fav Menus, Grid Generation, Inventory Visibility 
    /// & much more! 
    /// </summary>
    public class InventoryProManager : MonoBehaviour
    {
        /// <summary>
        /// Defines the style of the Inventory & its behaviour.
        /// Grid Slots take 1 space from the Inventory (1x1 Slots only)
        /// while Tetris slots can take different sizes ( 2x1, 2x2, 3x1, 3x2, etc... )
        /// </summary>
        [System.Serializable]
        public enum InventoryStyle
        {
            Grid, Tetris
        }

        /// <summary>
        /// Defines the behaviour when right clicking on an Inventory Slot.
        /// None: Nothing Happens. Right click behaviour is disabled.
        /// Drop: The Item will be dropped.
        /// ContextMenu: Context Menu appears, allowing multiple options ( Recommended )
        /// </summary>
        [System.Serializable]
        public enum SlotRightClickEvent
        {
            None, Drop, ContextMenu
        }

        [System.Serializable]
        public class InventoryEvents
        {
            public UnityEvent onInventoryGenerated, onOpenInventory, onCloseInventory, onHotbarGenerated, onOpenChest, onCloseChest,
                onSlotBeginDrag, onSlotEndDrag, onRotateItem, onDropItem, onHighlightSlot, onStackItems;
        }

        [SerializeField, Tooltip("Defines the style of the Inventory & its behaviour.\n\n" +
            "Grid Slots take 1 space from the Inventory (1x1 Slots only)\n\n" +
            "while Tetris slots can take different sizes ( 2x1, 2x2, 3x1, 3x2, etc... )")] private InventoryStyle inventoryStyle;
        [SerializeField, Tooltip("If true, the Player will be able to pin favorite Items in a Radial Menu for Quick Access & usage\n\n")] private bool useFavouritesRadialMenu;
        [SerializeField, Tooltip("Defines the behaviour when right clicking on an Inventory Slot.\n\n" +
            "None: Nothing Happens. Right click behaviour is disabled.\n\n" +
            "Drop: The Item will be dropped.\n\n" +
            "ContextMenu: Context Menu appears, allowing multiple options ( Recommended )")] SlotRightClickEvent slotRightClickEvent;
        [SerializeField] private InteractManager interactManager;
        [SerializeField] private TooltipManager tooltipManager;
        [SerializeField] private FavItemsMenu favItemsMenu;
        [SerializeField] private bool rightClickUnpinsItem;
        [SerializeField] private GridGenerator gridGenerator;
        [SerializeField, Tooltip("if true & the hotbhar is full of weapons: Weapon will be stored in the Inventory.\n\n" +
            "If false & the hotbar is full of weapons: Weapon will be swapped in the hotbar for the current weapon.")] private bool storeWeaponsIfHotbarFull;
        [SerializeField, Tooltip("Attach the generic pickeable object here")] private Pickeable weaponGenericPickeable, attachmentGenericPickeable, bulletsGenericPickeable, itemGenericPickeable;
        [SerializeField, Tooltip("Reference to the Button that closes the Inventory on press. This action is automatically assigned.")] private Button closeButton;
        [SerializeField, Tooltip("Reference to the Area that deletes Items from the Inventory on drop. This action is automatically assigned.")] private RectTransform trashButton;
        [SerializeField, Tooltip("UI that appears when using a Gamepad with the Inventory")] private GameObject gamepadSpecificUI;
        [SerializeField, Tooltip("When highlighted, slots will take on a tint of this color.")] private Color highlightSlotColor;
        [SerializeField, Tooltip("When available while dragging a slot, slots will take on a tint of this color.")] private Color availableSlotColor;
        [SerializeField, Tooltip("When occupied while dragging a slot, slots will take on a tint of this color.")] private Color occupiedSlotColor;
        [SerializeField, Tooltip("UI that appears when examining an item. Check ExamineItem.cs for more information.")] private GameObject examinationUI;
        [SerializeField, Tooltip("Text to display examined name.")] private TextMeshProUGUI examinationText;
        [SerializeField, Tooltip("If true, and lootAllChestButton is not null, lootAllChestButton will loot the entire chest on click. This feature" +
            "is separate from Context Menu�s Loot All Button")] private bool allowLootAllChest;
        [SerializeField] private Button lootAllChestButton;
        [SerializeField, Tooltip("If slotRightClickEvent is set to ContextMenu, this object will be enabled when right clicking on slots, allowing" +
            "multiple actions and additional functionality.")] private ContextMenu contextMenu;
        [SerializeField, Tooltip("To avoid displaying the ContextMenu exactly where the Mouse is placed, we can add a position offset (horizontal, vertical)")] private Vector2 contextMenuOffset;
        [SerializeField, Range(0f, 50f)] private float contextMenuRaycastPadding = 30;
        [SerializeField] private InventoryEvents events;

        [SerializeField] private AudioClip openInventorySFX;
        [SerializeField] private AudioClip highlightItemSFX;
        [SerializeField] private AudioClip rotateItemSFX;
        [SerializeField] private AudioClip placeItemSFX;
        [SerializeField] private AudioClip contextMenuClickSFX;

        // NAVIGATION INPUT ACTIONS
        [SerializeField] private InventoryProManagerInputActions invInputActions;
        public Action<InventorySlot> onSlotRightClick;

        // INTERNAL USE & REFERENCES
        private int itemRotation = 0;
        private bool isInventoryOpen;
        private bool isFavRadialMenuOpen; 
        private WeaponController weaponController;
        private PlayerMovement playerMovement;
        private PlayerStats playerStats;
        private WeightSystem weightSystem;

        // PUBLIC ACCESS
        public InventorySlot highlightedSlot;
        public InventorySlot draggedSlot;

        // GETTERS & ACCESSORS
        // All these are accessible to read but not to write
        public InteractManager _InteractManager => interactManager;
        public Pickeable WeaponGenericPickeable => weaponGenericPickeable;
        public Pickeable AttachmentGenericPickeable => attachmentGenericPickeable;
        public Pickeable BulletsGenericPickeable => bulletsGenericPickeable;
        public Pickeable ItemGenericPickeable => itemGenericPickeable;
        public InventoryStyle _InventoryStyle => inventoryStyle;
        public bool UseFavouritesRadialMenu => useFavouritesRadialMenu;
        public bool StoreWeaponsIfHotbarFull => storeWeaponsIfHotbarFull;
        public Color AvailableSlotColor => availableSlotColor;
        public Color OccupiedSlotColor => occupiedSlotColor;
        public TooltipManager _TooltipManager => tooltipManager;
        public GridGenerator _GridGenerator => gridGenerator;
        public FavItemsMenu _FavItemsMenu => favItemsMenu;

        public bool RightClickUnpinsItem => rightClickUnpinsItem;
        public InventorySlot[,] InventorySlots => gridGenerator.InventorySlots;
        public InventorySlot[,] ChestSlots => gridGenerator.ChestSlots;
        public InventorySlot[] HotbarSlots => gridGenerator.HotbarSlots;
        public RectTransform TrashButton => trashButton;
        public AudioClip ContextMenuClickSFX => contextMenuClickSFX;
        public SlotRightClickEvent _SlotRightClickEvent => slotRightClickEvent;
        public List<InventorySlot> highlightedSlotArea = new List<InventorySlot>();
        public bool IsInventoryOpen => isInventoryOpen;
        public bool IsFavRadialMenuOpen => isFavRadialMenuOpen;

        public bool AllowLootAllChest => allowLootAllChest; 
        public InventoryEvents Events => events;

        public WeaponController _WeaponController => weaponController;

        public static InventoryProManager instance;

        private void Awake()
        {
            // Handle singleton
            if (instance == null) instance = this;
            else Destroy(this.gameObject);
        }

        private void Start()
        {
            InitializeSettings();
            CloseInventory();
            ToggleDeviceSpecificUI(false);
        }
        private void InitializeSettings()
        {
            // Gahter References
            weaponController = interactManager.GetComponent<WeaponController>();
            playerMovement = interactManager.GetComponent<PlayerMovement>();
            playerStats = interactManager.GetComponent<PlayerStats>();
            weightSystem = playerStats.WeightSystem;

            // Initialize the Pinned Menu for Quick Access if useFavouritesRadialMenu allows it
            if (useFavouritesRadialMenu)
            {
                favItemsMenu.gameObject.SetActive(true);
                favItemsMenu.Initialize(this, weaponController);
                favItemsMenu.gameObject.SetActive(false);
            }

            // Initialize Grid Generator References
            gridGenerator.Initialize(weaponController);

            // Handle On Slot Right Click Actions Subscription
            onSlotRightClick = slotRightClickEvent switch
            {
                SlotRightClickEvent.None => null,
                SlotRightClickEvent.Drop => DropOutsideInventory,
                SlotRightClickEvent.ContextMenu => ShowContextMenu,
                _ => null,
            };

            // Listeners & Events
            interactManager.events.OnFinishedInteraction.AddListener(OnFinishedInteraction);
            interactManager.events.onDrop.AddListener(OnDropHotbar);
            weaponController.events.OnEquipWeapon.AddListener(() => CalculateTotalBullets(overrideCurrentBullets: true));
            weaponController.events.OnShoot.AddListener(UpdateBulletCountOnShooting);
            weaponController.events.OnFinishReload.AddListener(OnWeaponReload);

            InputManager.onTogglePause += CloseInventory;
            InputManager.onTogglePause += CloseFavRadialMenu;
            InputManager.onInventoryOpenPressed += ToggleInventoryVisibility;
            InputManager.onInventoryFavOpenPressed += ToggleFavRadialMenu;

            closeButton.onClick.AddListener(CloseInventory);
            if(allowLootAllChest && lootAllChestButton != null) lootAllChestButton.onClick.AddListener(LootAllChest);

            // If we are not pretending to save the game, GridGenerator should generate the inventory & Hotbar and then populate it, otherwise we�ll allow
            // GameDataManager to do it based on the saved & loaded data.
            if (GameDataManager.instance == null || DataPersistenceManager.instance.SelectedProfileIdIsNull())
            {
                gridGenerator.GenerateInventory();
                gridGenerator.PopulateInventory();
                gridGenerator.GenerateHotbar();
            }

            InitializeControllerNavigation();
        }

        private void InitializeControllerNavigation()
        {
            // Radial Only Actions
            if (UseFavouritesRadialMenu)
            {
                invInputActions.navigateRadialAction.performed += ctx => favItemsMenu.RadialNavigation(ctx);
                invInputActions.selectAction.performed += ctx => {
                    var pointerData = new PointerEventData(EventSystem.current) // Simulates Left Click for Controllers
                    {
                        button = PointerEventData.InputButton.Left
                    };

                    favItemsMenu.SelectedSlot?.ClickAction(pointerData);
                };
                invInputActions.navigateRadialAction.Enable();
            }
            invInputActions.navigateLeftAction.performed += ctx => NavigateToSlot(0, -1);
            invInputActions.navigateRightAction.performed += ctx => NavigateToSlot(0, 1);
            invInputActions.navigateUpAction.performed += ctx => NavigateToSlot(-1, 0);
            invInputActions.navigateDownAction.performed += ctx => NavigateToSlot(1, 0);
            invInputActions.selectAction.performed += ctx => SelectSlotController();
            invInputActions.rotateItemAction.performed += ctx => RotateItem(itemRotation += 90);
            invInputActions.rotateItemLeftAction.performed += ctx => RotateItem(itemRotation -= 90);
            invInputActions.rotateItemRightAction.performed += ctx => RotateItem(itemRotation += 90);

            // Shared Actions
            invInputActions.closeAction.performed += ctx => CloseInventory();
            invInputActions.dropAction.performed += ctx =>
            {
                if (isInventoryOpen) DropOutsideInventory(highlightedSlot.GetAnchorSlot());
            };
            invInputActions.anyKeyAction = new InputAction(type: InputActionType.PassThrough, binding: "<Keyboard>/anyKey");
            invInputActions.anyKeyAction.performed += ctx => ToggleDeviceSpecificUI(false);
            invInputActions.mouseLeftClickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/press");
            invInputActions.mouseLeftClickAction.performed += ctx => ToggleDeviceSpecificUI(false);
            invInputActions.mouseMoveAction = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta");
            invInputActions.mouseMoveAction.performed += ctx => ToggleDeviceSpecificUI(false);

            invInputActions.Enable();
        }
        private void OnDisable()
        {
            interactManager.events.OnFinishedInteraction.RemoveListener(OnFinishedInteraction);
            interactManager.events.onDrop.RemoveListener(OnDropHotbar);
            weaponController.events.OnEquipWeapon.RemoveAllListeners();
            weaponController.events.OnShoot.RemoveListener(UpdateBulletCountOnShooting);
            weaponController.events.OnFinishReload.RemoveListener(OnWeaponReload);

            InputManager.onTogglePause -= CloseInventory;
            InputManager.onTogglePause -= CloseFavRadialMenu;
            InputManager.onInventoryOpenPressed -= ToggleInventoryVisibility;
            InputManager.onInventoryFavOpenPressed -= ToggleFavRadialMenu;

            closeButton.onClick.RemoveListener(CloseInventory);

            onSlotRightClick = null;

            invInputActions.Disable(); // Disable all actions
        }

        /// <summary>
        /// This method is used to select a slot in the Inventory, Chest or Hotbar, only if using Controller/Gamepad
        /// </summary>
        private void SelectSlotController()
        {
            if (!highlightedSlot) return;

            if (highlightedSlot.HasValidItem || highlightedSlot.anchorSlot)
            {
                // Toggle off dragging if we are already dragging a slot and we select a new slot
                if (draggedSlot)
                {
                    OnEndDrag(false);
                    draggedSlot = null;
                }
                else // If we are not dragging, start drag.
                {
                    draggedSlot = highlightedSlot.GetAnchorSlot();
                    draggedSlot.OnBeginDrag(null);
                }
            }
            else if (draggedSlot) // Place Item
            {
                OnEndDrag(false);
                draggedSlot = null; 
            }

            // Enable Gamepad specific UI since we are using a Gamepad right now.
            ToggleDeviceSpecificUI(true);
        }

        /// <summary>
        /// Used by Gamepad Navigation. Highlights a Slot form Inventory, Hotbar or Chest. This is equivalent to hovering it with the mouse, but with the gamepad.
        /// </summary>
        private void NavigateToSlot(int rowChange, int colChange)
        {
            if (!gridGenerator.gameObject.activeSelf) return;

            // Enable Gamepad specific UI since we are using a Gamepad right now.
            ToggleDeviceSpecificUI(true);

            // If no slot is highlighted yet, default to the first inventory slot.
            if (highlightedSlot == null) UpdateSlotAndSelection(InventorySlots[0, 0]);

            if (highlightedSlot.IsHotbarSlot) NavigateFromHotbar(rowChange, colChange);
            else if (highlightedSlot.IsInventorySlot) NavigateFromInventory(rowChange, colChange);
            else if (highlightedSlot.IsChestSlot) NavigateFromChest(rowChange, colChange);
        }

        private void NavigateFromHotbar(int rowChange, int colChange)
        {
            // Moving vertically from the hotbar moves into the inventory.
            if (rowChange > 0)
            {
                // First inventory row
                gridGenerator.selectedControllerSlotRow = 0;
                UpdateSlotAndSelection(InventorySlots[gridGenerator.selectedControllerSlotRow, gridGenerator.selectedControllerSlotColumn]);
                return;
            }
            else if (rowChange < 0)
            {
                // Last inventory row
                gridGenerator.selectedControllerSlotRow = _GridGenerator.Rows - 1;
                UpdateSlotAndSelection(InventorySlots[gridGenerator.selectedControllerSlotRow, gridGenerator.selectedControllerSlotColumn]);
                return;
            }

            // Otherwise, cycle columns within the hotbar
            int newHotbarIndex = gridGenerator.selectedControllerSlotColumn + colChange;
            // Go to the first slot
            if (newHotbarIndex >= HotbarSlots.Length) newHotbarIndex = 0;
            else if (newHotbarIndex < 0) newHotbarIndex = HotbarSlots.Length - 1; // Go to the last slot

            gridGenerator.selectedControllerSlotColumn = newHotbarIndex;
            UpdateSlotAndSelection(HotbarSlots[newHotbarIndex]);
        }

        private void NavigateFromInventory(int rowChange, int colChange)
        {
            int newRow = gridGenerator.selectedControllerSlotRow + rowChange;
            int newColumn = gridGenerator.selectedControllerSlotColumn + colChange;

            // If moving vertically out of the inventory, navigate & cycle to the hotbar
            if (newRow < 0 || newRow >= _GridGenerator.Rows)
            {
                // Hotbar row indicator ( row set to -1 )
                gridGenerator.selectedControllerSlotRow = -1;
                UpdateSlotAndSelection(HotbarSlots[0]);
                return;
            }

            // Cycle columns within the inventory
            if (newColumn >= _GridGenerator.Columns)
            {
                newColumn = 0;
            }
            else if (newColumn < 0)
            {
                // If navigating to the left & a chest is opened, handle chest navigation
                if (ChestSlots?.Length > 0)
                {
                    newColumn = _GridGenerator.CachedChest.ChestSize.x;
                    if (newRow > _GridGenerator.CachedChest.ChestSize.x - 1)
                        newRow = _GridGenerator.CachedChest.ChestSize.x - 1;

                    gridGenerator.selectedControllerSlotRow = newRow;
                    gridGenerator.selectedControllerSlotColumn = newColumn;
                    UpdateSlotAndSelection(ChestSlots[newRow, newColumn]);
                    return;
                }
                else
                {
                    newColumn = _GridGenerator.Columns - 1;
                }
            }

            gridGenerator.selectedControllerSlotRow = newRow;
            gridGenerator.selectedControllerSlotColumn = newColumn;
            UpdateSlotAndSelection(InventorySlots[newRow, newColumn]);
        }

        private void NavigateFromChest(int rowChange, int colChange)
        {
            int newRow = gridGenerator.selectedControllerSlotRow + rowChange;
            int newColumn = gridGenerator.selectedControllerSlotColumn + colChange;

            if (newRow > _GridGenerator.CachedChest.ChestSize.x - 1)
                newRow = 0;

            // Cycle columns within the chest.
            if (newColumn >= _GridGenerator.CachedChest.ChestSize.y)
            {
                newColumn = 0;
                gridGenerator.selectedControllerSlotRow = newRow;
                gridGenerator.selectedControllerSlotColumn = newColumn;
                UpdateSlotAndSelection(InventorySlots[newRow, newColumn]);
                return;
            }
            else if (newColumn < 0)
            {
                newColumn = _GridGenerator.Columns - 1;
                gridGenerator.selectedControllerSlotRow = newRow;
                gridGenerator.selectedControllerSlotColumn = newColumn;
                UpdateSlotAndSelection(InventorySlots[newRow, newColumn]);
                return;
            }

            gridGenerator.selectedControllerSlotRow = newRow;
            gridGenerator.selectedControllerSlotColumn = newColumn;
            UpdateSlotAndSelection(ChestSlots[newRow, newColumn]);
        }

        /// <summary>
        /// Updates the highlighted slot and the associated UI/tooltip
        /// </summary>
        private void UpdateSlotAndSelection(InventorySlot newSlot)
        {
            highlightedSlot?.OnPointerExit(null);
            HighlightSlot(newSlot);
            highlightedSlot.OnPointerEnter(null);
            tooltipManager.SetPosition(highlightedSlot.transform.position);
            EventSystem.current.SetSelectedGameObject(highlightedSlot.gameObject);
        }

        private void ToggleDeviceSpecificUI(bool isGamepad)
        {
            if (closeButton) closeButton?.gameObject.SetActive(!isGamepad);
            if(trashButton) trashButton?.gameObject.SetActive(!isGamepad);
            gamepadSpecificUI?.gameObject.SetActive(isGamepad);
        }

        /// <summary>
        /// Highlights/Selects a slot & enables Visual Indicators.
        /// </summary>
        /// <param name="slot">InventorySlot to highlight</param>
        public void HighlightSlot(InventorySlot slot)
        {
            highlightedSlot?.Unselect();
            highlightedSlot = slot;
            highlightedSlot?.Select();

            if (highlightedSlot)
            {
                events.onHighlightSlot?.Invoke();
                SoundManager.Instance.PlaySound(highlightItemSFX, 0, 0, false, 0);
            }
        }

        /// <summary>
        /// If Inventory is open, close it. If it is closed, open it.
        /// Inventory cannot be opened if the game is paused or the player is dead.
        /// </summary>
        private void ToggleInventoryVisibility()
        {
            if (PauseMenu.isPaused || playerStats.IsDead) return;

            if (!isInventoryOpen && ( !interactManager.inspecting && interactManager.realtimeAttachmentCustomization || !interactManager.realtimeAttachmentCustomization)) OpenInventory();
            else CloseInventory();
        }

        /// <summary>
        /// Opens the Inventory & Enables UI
        /// </summary>
        public void OpenInventory()
        {
            // Fav Pinned Menu & Inventory need to be alternated, both cannot be opened at the same time.
            if (isFavRadialMenuOpen) CloseFavRadialMenu();
            isInventoryOpen = true;

            // Player shouldn�t move or perform any UI unrelated action.
            InputManager.ToggleUIControls(true);
            PauseMenu.Instance.stats.LoseControl();
            playerMovement.StopSpeedlines();
            UIController.instance.UnlockMouse();
            UIController.instance.crosshair.SetVisibility(false);
            contextMenu.HideContextMenu();

            gridGenerator.gameObject.SetActive(true);
            gridGenerator.selectedControllerSlotRow = 0;
            gridGenerator.selectedControllerSlotColumn = 0;
            // For gamepad navigation purposes.
            if(EventSystem.current != null && Gamepad.current != null) EventSystem.current.SetSelectedGameObject(InventorySlots[gridGenerator.selectedControllerSlotRow, gridGenerator.selectedControllerSlotColumn].gameObject);

            draggedSlot = null;
            HighlightSlot(null);

            events.onOpenInventory?.Invoke();
            SoundManager.Instance.PlaySound(openInventorySFX, 0, 0, false, 0);
        }
        /// <summary>
        /// Closes the Inventory & Disables UI
        /// </summary>
        public void CloseInventory()
        {
            isInventoryOpen = false;
            gridGenerator.gameObject.SetActive(false);

            // In case the Inventory Toggle Key is set to the same as Drop, prevent from Dropping the current weapon
            InputManager.dropping = false;
            InputManager.crouching = false;
            // Prevents from shooting unintentionally after closing the inventory
            InputManager.shooting = false;
            CamShake.instance.Trauma = 0;

            // Grant Control back to the player
            PauseMenu.Instance?.stats?.CheckIfCanGrantControl();
            UIController.instance.LockMouse();
            UIController.instance.crosshair.SetVisibility(true);

            tooltipManager.Disable();

            // Chest UI is binded to the Inventory, Close the Chest in case it was open.
            gridGenerator.CloseChest();
            InputManager.ToggleGameControls(true);
            InputManager.ToggleUIControls(false);

            draggedSlot = null;
            HighlightSlot(null);

            contextMenu.HideContextMenu();

            events.onCloseInventory?.Invoke();
        }

        /// <summary>
        /// If FavPinned Menu is open, close it. If it is closed, open it.
        /// FavPinned Menu cannot be opened if the game is paused or the player is dead.
        /// </summary>
        private void ToggleFavRadialMenu()
        {
            if (PauseMenu.isPaused || playerStats.IsDead || favItemsMenu == null) return;

            if (isInventoryOpen) CloseInventory();

            if (!isFavRadialMenuOpen && (!interactManager.inspecting && interactManager.realtimeAttachmentCustomization || !interactManager.realtimeAttachmentCustomization)) OpenFavRadialMenu();
            else CloseFavRadialMenu();
        }

        private void OpenFavRadialMenu()
        {
            if (favItemsMenu == null) return;

            isFavRadialMenuOpen = true;
            favItemsMenu.gameObject.SetActive(true);

            // Player shouldn�t move or perform any UI unrelated action.
            InputManager.ToggleUIControls(true);
            PauseMenu.Instance.stats.LoseControl();
            UIController.instance.UnlockMouse();
            UIController.instance.crosshair.SetVisibility(false);
            contextMenu.HideContextMenu();

            // For gamepad navigation purposes.
            favItemsMenu.selectedControllerSlotIndex = 0;
            if (favItemsMenu.SelectedSlot != null) EventSystem.current.SetSelectedGameObject(favItemsMenu.SelectedSlot.gameObject);
        }
        private void CloseFavRadialMenu()
        {
            if (favItemsMenu == null) return;

            isFavRadialMenuOpen = false;
            favItemsMenu.gameObject.SetActive(false);

            // Grant Control back to the player
            PauseMenu.Instance?.stats?.CheckIfCanGrantControl();
            UIController.instance.LockMouse();
            UIController.instance.crosshair.SetVisibility(true);
        }
        
        /// <summary>
        /// This method is called when the Player interacts with an Interactable.
        /// </summary>
        private void OnFinishedInteraction()
        {
            if (interactManager.HighlightedInteractable is WeaponPickeable)  // Handle Interaction with WeaponPickeable.
            {
                for (int i = 0; i < weaponController.inventorySize; i++)
                {
                    if (weaponController.inventory[i])
                    {
                        WeaponPickeable weaponPickeable = (WeaponPickeable)interactManager.HighlightedInteractable;
                        UpdateHotbarSlot(i, weaponPickeable.Barrel, weaponPickeable.Scope, weaponPickeable.Stock, weaponPickeable.Grip, weaponPickeable.Magazine, weaponPickeable.Flashlight,
                            weaponPickeable.Laser);
                        OnAddedItem(weaponPickeable.weapon);
                    }
                }
                CalculateTotalBullets(overrideCurrentBullets: true);
            }
            else if (interactManager.HighlightedInteractable is AttachmentPickeable)// Handle Interaction with AttachmentPickeable.
            {
                InventorySlot currentInventorySlot = HotbarSlots[weaponController.currentWeapon];
                AttachmentPickeable attachmentPickeable = interactManager.HighlightedInteractable as AttachmentPickeable;
                if (attachmentPickeable.CheckCompatibleAttachment(weaponController))
                {
                    switch (attachmentPickeable.Atc)
                    {
                        case Scope scope: currentInventorySlot.slotData.scope = attachmentPickeable.attachmentIdentifier; break;
                        case Barrel barrel: currentInventorySlot.slotData.barrel = attachmentPickeable.attachmentIdentifier; break;
                        case Stock stock: currentInventorySlot.slotData.stock = attachmentPickeable.attachmentIdentifier; break;
                        case Grip grip: currentInventorySlot.slotData.grip = attachmentPickeable.attachmentIdentifier; break;
                        case Magazine magazine: currentInventorySlot.slotData.magazine = attachmentPickeable.attachmentIdentifier; break;
                        case Flashlight flashlight: currentInventorySlot.slotData.flashlight = attachmentPickeable.attachmentIdentifier; break;
                        case Laser laser: currentInventorySlot.slotData.laser = attachmentPickeable.attachmentIdentifier; break;
                    }
                }
                // OnAddedItem(attachmentPickeable.attachmentIdentifier);
            }
            else if (interactManager.HighlightedInteractable is BulletsPickeable) // Handle Interaction with BulletsPickeable.
            {
                // var prevBulletsCount = weaponController.id.totalBullets;
                CalculateTotalBullets(overrideCurrentBullets: false);
                // weaponController.id.totalBullets = gridGenerator.GatherBulletsInInventory();
                // Debug.Log($"было {prevBulletsCount} пуль, стало - {weaponController.id.totalBullets}");
                // OnAddedItem((interactManager.HighlightedInteractable as BulletsPickeable).bulletsSO, weaponController.id.totalBullets - prevBulletsCount);
                // Debug.Log($"new weight: " + weightSystem.CurrentWeight);
            }
        }

        /// <summary>
        /// Updates GridGenerator HotbarSlot information so it coordinates with inventory in WeaponController.
        /// </summary>
        public void UpdateHotbarSlot(int slotIndex, AttachmentIdentifier_SO Barrel, AttachmentIdentifier_SO Scope, AttachmentIdentifier_SO Stock, AttachmentIdentifier_SO Grip,
            AttachmentIdentifier_SO Magazine, AttachmentIdentifier_SO Flashlight, AttachmentIdentifier_SO Laser)
        {
            InventorySlot currentInventorySlot = HotbarSlots[slotIndex];
            currentInventorySlot.slotData.item = weaponController.inventory[slotIndex].weapon;
            currentInventorySlot.slotData.amount = 1;
            currentInventorySlot.UpdateSlotGraphics();

            currentInventorySlot.slotData.bulletsLeftInMagazine = weaponController.inventory[slotIndex].bulletsLeftInMagazine;
            currentInventorySlot.slotData.totalBullets = weaponController.inventory[slotIndex].totalBullets;
            currentInventorySlot.slotData.barrel = Barrel;
            currentInventorySlot.slotData.scope = Scope;
            currentInventorySlot.slotData.stock = Stock;
            currentInventorySlot.slotData.grip = Grip;
            currentInventorySlot.slotData.magazine = Magazine;
            currentInventorySlot.slotData.flashlight = Flashlight;
            currentInventorySlot.slotData.laser = Laser;
        }

        /// <summary>
        /// Removes an Item from the Inventory and Instantiates a Pickeable
        /// </summary>
        /// <param name="slot">Slot to remove.</param>

        public void DropOutsideInventory(InventorySlot slot)
        {
            // if the slot is empty, there is nothing to drop
            if (!slot.HasValidItem) return;

            // Instantiate the Pickeable & save it so its spawned next time we load the game
            Transform orientation = playerMovement.orientation;
            Vector3 dropPosition = orientation.position + orientation.forward * interactManager.DroppingDistance;
            Quaternion dropRotation = orientation.rotation;

            Pickeable pickeable = InstantiatePickeable(slot, dropPosition, dropRotation);
            // OnRemoveItem(slot.slotData.item, slot.slotData.amount);

#if SAVE_LOAD_ADD_ON
            if (pickeable != null)
                pickeable.SaveInstance();
#endif

            // Update Favorites Menu before deleting to avoid missmatches
            if (useFavouritesRadialMenu) favItemsMenu.UnpinFromFavMenu(slot);
            DeleteItem(slot);
            CalculateTotalBullets(overrideCurrentBullets: false);

            tooltipManager.Disable();
            events.onDropItem?.Invoke();
        }

        // This is called when an Item is dropped outside the Inventory from the hotbar.
        // Updates the HotbarSlots visuals from GridGenerator to ensure it is up-to-date with inventory in WeaponController.
        private void OnDropHotbar(Pickeable pick)
        {
            InventorySlot hotbarSlot = HotbarSlots[weaponController.currentWeapon];
            // OnRemoveItem(hotbarSlot.slotData.item, hotbarSlot.slotData.amount);
            if (useFavouritesRadialMenu) favItemsMenu.UnpinFromFavMenu(hotbarSlot);
            DeleteItem(hotbarSlot);
            // hotbarSlot.slotData = new SlotData();
            hotbarSlot.UpdateSlotGraphics();

            if (GameDataManager.instance == null) return;
            
            GameDataManager.instance.OnDropHotbar(pick);
        }

        /// <summary>
        /// Re-calculates the available bullets in the inventory that can be used by weapons with limited magazines
        /// </summary>
        /// <param name="overrideCurrentBullets">Overrides current Weapon Magazine with available bullets.</param>
        public void CalculateTotalBullets(bool overrideCurrentBullets, Item_SO bullet = null)
        {
            if (weaponController.weapon == null || !weaponController.weapon.limitedMagazines) return;
            if (overrideCurrentBullets) weaponController.id.bulletsLeftInMagazine = gridGenerator.HotbarSlots[weaponController.currentWeapon].slotData.bulletsLeftInMagazine;
            weaponController.id.totalBullets = gridGenerator.GatherBulletsInInventory();
        }

        // Called when shooting
        private void UpdateBulletCountOnShooting()
        {
            if (HotbarSlots.Length <= 0 || weaponController.weapon == null) return;
            HotbarSlots[weaponController.currentWeapon].slotData.bulletsLeftInMagazine = weaponController.id.bulletsLeftInMagazine;
        }

        // Called when reloading, updates bullets in Inventory to ensure Bullet Count values are up-to-date between the Weapons & the Inventory
        private void OnWeaponReload()
        {
            if (!weaponController.weapon.limitedMagazines) return;

            int previousAmount = gridGenerator.GatherBulletsInInventory();
            int currentAmount = weaponController.id.totalBullets;

            int difference = previousAmount - currentAmount;
            gridGenerator.ReduceBulletsInInventory(difference);

            WeaponIdentification id = weaponController.inventory[weaponController.currentWeapon];
            HotbarSlots[weaponController.currentWeapon].slotData.bulletsLeftInMagazine = id.bulletsLeftInMagazine;
            HotbarSlots[weaponController.currentWeapon].slotData.totalBullets = id.totalBullets;
        }

        // Instantiates the correct Pickeable based on the slot item type (Weapon_SOs create WeaponPickeables, AttachmentIdentifier_SOs => AttachmentPickeable,
        // BuletsItem_SO => BulletsPickeable, Item_SO => InventoryItemPickeable )
        private Pickeable InstantiatePickeable(InventorySlot slot, Vector3 position, Quaternion rotation)
        {
            if (slot.IsItemWeapon)
            {
                var pick = Instantiate(weaponGenericPickeable, position, rotation) as WeaponPickeable;
                pick.dropped = true;
                pick.currentBullets = slot.slotData.bulletsLeftInMagazine;
                pick.totalBullets = slot.slotData.totalBullets;
                pick.weapon = (Weapon_SO)slot.slotData.item;
                pick.SetPickeableAttachments(slot.slotData.barrel, slot.slotData.scope, slot.slotData.stock, slot.slotData.grip, slot.slotData.magazine, slot.slotData.flashlight, slot.slotData.laser);
                pick.GetVisuals();
                return pick;
            }
            else if (slot.IsItemBullets)
            {
                var pick = Instantiate(bulletsGenericPickeable, position, rotation) as BulletsPickeable;
                pick.SetBullets((BulletsItem_SO)slot.slotData.item, slot.slotData.amount);
                return pick;
            }
            else if (slot.IsItemAttachment)
            {
                var pick = Instantiate(attachmentGenericPickeable, position, rotation) as AttachmentPickeable;
                pick.dropped = true;
                pick.attachmentIdentifier = (AttachmentIdentifier_SO)slot.slotData.item;
                pick.GetVisuals();
                return pick;
            }
            else
            {
                var pick = Instantiate(itemGenericPickeable, position, rotation) as InventoryItemPickeable;
                pick.SetItem(slot.slotData.item, slot.slotData.amount);
                return pick;
            }
        }

        /// <summary>
        /// Used by ContextMenu, separates half of the amounf of the passed slot, and drops it outside the Inventory.
        /// </summary>
        /// <param name="slot">Slot to split</param>
        public void SplitOutsideInventory(InventorySlot slot)
        {
            if (slot.slotData?.item == null) return;

            Transform orientation = playerMovement.orientation;
            int amount = (int)Math.Floor((double)slot.slotData.amount / 2);

            if (slot.slotData.item is BulletsItem_SO)
            {
                BulletsPickeable pick = Instantiate(bulletsGenericPickeable, orientation.position + orientation.forward * interactManager.DroppingDistance, orientation.rotation) as BulletsPickeable;
                pick.SetBullets((BulletsItem_SO)slot.slotData.item, amount);
            }
            else
            {
                InventoryItemPickeable pick = Instantiate(itemGenericPickeable, orientation.position + orientation.forward * interactManager.DroppingDistance, orientation.rotation) as InventoryItemPickeable;
                pick.SetItem(slot.slotData.item, amount);
            }

            // Reduce the amount and reflect the changes in the UI
            slot.slotData.amount -= amount;
            Vector2Int size = slot.GetItemSize();
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    InventorySlot _slot = slot.IsInventorySlot ? InventorySlots[slot.row + i, slot.col + j] :
                        ChestSlots[slot.row + i, slot.col + j];
                    _slot.UpdateSlotGraphics();
                }
            }

            CalculateTotalBullets(overrideCurrentBullets: false);
        }

        // Enables Context Menu
        private void ShowContextMenu(InventorySlot anchor) => contextMenu.ShowContextMenu(anchor, (Vector3)Mouse.current?.position.ReadValue() + (Vector3)contextMenuOffset, contextMenuRaycastPadding);

        /// <summary>
        /// Deletes an item from the Inventory / Hotbar / Chest
        /// </summary>
        /// <param name="slot">InventorySlot to delete</param>
        public void DeleteItem(InventorySlot slot)
        {
            SlotData cachedSlotData = slot.slotData;
            OnRemoveItem(slot.slotData.item, slot.slotData.amount);

            if (slot.IsHotbarSlot)
            {
                if (slot.col == weaponController.currentWeapon && weaponController.inventory[slot.col])
                    weaponController.ReleaseCurrentWeapon();
                else
                {
                    weaponController.ForceAimReset();
                    Destroy(weaponController.inventory[slot.col]?.gameObject);
                    weaponController.inventory[slot.col] = null;
                    weaponController.weaponsInventoryUISlots[slot.col].SetWeapon(null);
                }

                if (useFavouritesRadialMenu) favItemsMenu.UnpinFromFavMenu(slot);
                slot.ClearSlot();
            }
            else
            {
                if (useFavouritesRadialMenu) favItemsMenu.UnpinFromFavMenu(slot);
                gridGenerator.ClearSlotArea(slot);
            }

            // Update Bullets Count in case the deleted item was a Bullets Item
            // However, if it was a weapon, stop reloading in case we were reloading
            if (cachedSlotData.item is BulletsItem_SO) CalculateTotalBullets(overrideCurrentBullets: false);
            else if (cachedSlotData.item is Weapon_SO && weaponController.Reloading) weaponController.StopReload();

            ToastManager.Instance.ShowToast($"{cachedSlotData.item._name} {ToastManager.Instance.ItemDeleted}");
        }

        /// <summary>
        /// For Tetris Inventories only, rotates an item. 0 & 180 degress are considered as oriented. 90 & 270 are considered not oriented.
        /// </summary>
        /// <param name="rot">Rotation of the Slot in multiples of 90 from 0 to 360 ( 360 will be reset to 0 by default ) </param>
        public void RotateItem(int rot)
        {
            if (draggedSlot == null || inventoryStyle != InventoryStyle.Tetris)
                return;

            itemRotation = rot;
            if (itemRotation >= 360) itemRotation = 0;

            draggedSlot.slotData.isOriented = IsOriented();

            tooltipManager.Rotate(itemRotation);

            if (highlightedSlot != null)
            {
                ResetHighlightedSlotsArea(highlightedSlot.DefaultBackgroundColor, clearCurrentReferences: false);
                HighlightAvailableSlots(highlightedSlot);
                SoundManager.Instance.PlaySound(rotateItemSFX, 0, 0, false, 0);
            }

            events.onRotateItem?.Invoke();
        }

        /// <summary>
        /// Returns whether the currented rotation is oriented or not
        /// </summary>
        /// <returns>Orientation Bool</returns>
        public bool IsOriented() { return itemRotation == 0 || itemRotation == 180; }

        #region Examination

        /// <summary>
        /// Enables or Disables the Examination UI
        /// </summary>
        /// <param name="visible">This boolean controls the visibility state of the Examination UI</param>
        /// <param name="text">Name of the examined Item</param>
        public void SetExaminationUIVisibility(bool visible, string text)
        {
            examinationText.text = text + "\n\nE - Cancel \nF - Interact";
            examinationUI.SetActive(visible);
        }
        #endregion

        /// <summary>
        /// Resets highlighted area ( Whether available or occupied ) to the default background color.
        /// </summary>
        public void ResetHighlightedSlotsArea(Color defaultBackgroundColor, bool clearCurrentReferences)
        {
            highlightedSlotArea.Add(highlightedSlot);
            highlightedSlotArea.Add(draggedSlot);

            // Based on the size of the dragged item, store the colored slots into the highlightedSlots array so we can then recolor them back the default color
            Vector2Int itemSize = draggedSlot.HasValidItem ?
                          draggedSlot.GetItemSize() :
                          new Vector2Int(1, 1);

            int startRow = highlightedSlot.row;
            int startCol = highlightedSlot.col;
            for (int i = 0; i < itemSize.x; i++)
            {
                for (int j = 0; j < itemSize.y; j++)
                {
                    int targetRow = startRow + i;
                    int targetCol = startCol + j;

                    if (gridGenerator.IsSlotWithinBounds(targetRow, targetCol, InventorySlots))
                        SetBackgroundColor(InventorySlots[targetRow, targetCol], defaultBackgroundColor);
                }
            }

            SetSlotsColors(defaultBackgroundColor);
            highlightedSlotArea.Clear();

            if (clearCurrentReferences) ClearDraggedHighlightedReferences();

            CalculateTotalBullets(overrideCurrentBullets: false);
        }

        /// <summary>
        /// Helper function to reassign the slots for an item occupying multiple slots. Used by Tetris Inventories.
        /// </summary>
        public void ReassignSlots(InventorySlot slot)
        {
            // We can only reassign the slots if the anchor has a valid Item, otherwise the Slot should be 1x1, uninitialized.
            if (slot.GetAnchorSlot().HasValidItem)
            {
                InventorySlot anchor = slot.anchorSlot;
                Vector2Int itemSize = anchor.GetItemSize();

                int startRow = anchor.row;
                int startcol = anchor.col;
                for (int i = 0; i < itemSize.y; i++)
                {
                    for (int j = 0; j < itemSize.x; j++)
                    {
                        InventorySlot[,] inventorySlots = slot.IsInventorySlot ? InventorySlots : ChestSlots;

                        inventorySlots[startRow + i, startcol + j].anchorSlot = anchor;
                        inventorySlots[startRow + i, startcol + j].UpdateSlotGraphics();
                        highlightedSlotArea.Add(inventorySlots[startRow + i, startcol + j]);
                    }
                }

            }
        }

        /// <summary>
        /// In a Tetris Inventory, when trying to occupy some slots from the anchor, we first need to check if the Item fits in the target slot, based on the item size, to avoid 
        /// Grid overflow errors.
        /// </summary>
        /// <param name="targetSlot">Inventory Slot at which the anchor slot will be placed</param>
        /// <param name="itemSize">Item Size of the Item to place at targetSlot</param>
        public bool IsValidPosition(InventorySlot targetSlot, Vector2Int itemSize)
        {
            if (targetSlot == null) return false;
            InventorySlot[,] inventorySlots = targetSlot.IsInventorySlot ? InventorySlots : ChestSlots;

            if (!gridGenerator.IsSlotWithinBounds(targetSlot.row, targetSlot.col, itemSize, inventorySlots)) return false;

            int startRow = targetSlot.row;
            int startCol = targetSlot.col;
            for (int i = 0; i < itemSize.y; i++)
            {
                for (int j = 0; j < itemSize.x; j++)
                {
                    InventorySlot anchor = inventorySlots[startRow + i, startCol + j].anchorSlot;
                    if (IsSlotOccupied(anchor)) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns whether a slot is occupied or not. An item is occupied if it has a valid Anchor or Item assigned.
        /// </summary>
        /// <param name="anchor">Ensure to pass the Anchor of the Slot you want to check if it�s occupied.</param>
        public bool IsSlotOccupied(InventorySlot anchor)
        {
            return anchor && anchor != draggedSlot?.anchorSlot &&
                        (anchor != highlightedSlot || anchor.GetItemSize() != draggedSlot.anchorSlot?.GetItemSize());
        }

        /// <summary>
        /// Handles Highlighted Slot Area Tint based on slots availability. This is called when dragging an Item.
        /// </summary>
        /// <param name="slot"></param>
        public void HighlightAvailableSlots(InventorySlot slot)
        {
            // If we are dragging an attachment, and the target slot is a valid weapon, we want to color the weapon size in the available color.
            if (highlightedSlot?.GetAnchorSlot().slotData.item != null && CanAttachDraggedAttachmentToWeaponSlot())
            {
                HighlightAttachmentToWeaponSlots(slot);
            }
            else
            {
                // Normal Items Interaction ( No attachment-weapon pair type of Interaction )
                HighlightStandardItemSlots(slot);
            }
        }

        private void HighlightAttachmentToWeaponSlots(InventorySlot slot)
        {
            // Reset the slot area
            highlightedSlotArea.Clear();
            int startAnchorRow = slot.GetAnchorSlot().row;
            int startAnchorCol = slot.GetAnchorSlot().col;
            Vector2Int AnchorItemSize = slot.GetAnchorSlot().GetItemSize();
            // Beginning from the weapon anchor, add the entire weapon size inside the highlighted Slot Area
            for (int i = 0; i < AnchorItemSize.y; i++)
            {
                for (int j = 0; j < AnchorItemSize.x; j++)
                {
                    int targetRow = startAnchorRow + i;
                    int targetCol = startAnchorCol + j;

                    InventorySlot[,] inventorySlots = slot.IsInventorySlot ? InventorySlots : ChestSlots;

                    if (gridGenerator.IsSlotWithinBounds(targetRow, targetCol, inventorySlots))
                    {
                        highlightedSlotArea.Add(inventorySlots[targetRow, targetCol]);
                    }
                }
            }
            SetSlotsColors(AvailableSlotColor);
        }

        private void HighlightStandardItemSlots(InventorySlot slot)
        {
            bool draggedSlotHasItem = draggedSlot.HasValidItem;
            Vector2Int itemSize = draggedSlotHasItem ? draggedSlot.GetItemSize() : new Vector2Int(1, 1);
            bool isOccupied = false;
            int startRow = slot.row;
            int startCol = slot.col;
            // Check if the row and column are within bounds
            for (int i = 0; i < itemSize.y; i++)
            {
                for (int j = 0; j < itemSize.x; j++)
                {
                    int targetRow = startRow + i;
                    int targetCol = startCol + j;

                    InventorySlot[,] inventorySlots = slot.IsInventorySlot ? InventorySlots : ChestSlots;

                    if (gridGenerator.IsSlotWithinBounds(targetRow, targetCol, inventorySlots))
                    {
                        highlightedSlotArea.Add(inventorySlots[targetRow, targetCol]);
                        if (IsSlotOccupied(inventorySlots[targetRow, targetCol].anchorSlot)) isOccupied = true;
                    }
                }
            }

            if (!isOccupied) isOccupied = highlightedSlotArea.Count < itemSize.x * itemSize.y;

            Color color = isOccupied ? OccupiedSlotColor : AvailableSlotColor;

            SetSlotsColors(color);
        }

        /// <summary>
        /// Overrides the background color of all slots inside highlightedSlotArea
        /// </summary>
        /// <param name="color">Color to tint the background of the slots</param>
        public void SetSlotsColors(Color color)
        {
            foreach (var slot in highlightedSlotArea)
            {
                if (slot != null) SetBackgroundColor(slot, color);
            }
        }

        /// <summary>
        /// Overrides the background color of a specific slot.
        /// </summary>
        /// <param name="slot">Slot to modify</param>
        /// <param name="color">Color to tint the background of the slot. Usually availableColor, defaultbackgroundColor or occupiedColor</param>
        public void SetBackgroundColor(InventorySlot slot, Color color) => slot.BackgroundImage.color = color;

        // Gets called on Pointer Entering and Inventory Slot while dragging an Item
        public void SetAreaColorOnPointerEnter(InventorySlot slot)
        {
            // Calculate hotbar slots color first. If this is indeed a hotbar slot, do not calculate other slots avaiability.
            if (slot.IsHotbarSlot)
            {
                highlightedSlotArea.Add(highlightedSlot);
                bool isInvalidItemOrSlot = !draggedSlot.IsItemWeapon || (!draggedSlot.IsHotbarSlot && highlightedSlot.HasValidItem);
                bool canAttachDraggedToWeapon = CanAttachDraggedAttachmentToWeaponSlot();
                SetSlotsColors(canAttachDraggedToWeapon 
                    ? AvailableSlotColor 
                    : ( isInvalidItemOrSlot ? OccupiedSlotColor : AvailableSlotColor));
                return;
            }

            HighlightAvailableSlots(slot);
        }

        /// <summary>
        /// Resets dragged & highlighted slots
        /// </summary>
        public void ClearDraggedHighlightedReferences()
        {
            draggedSlot = null;
            HighlightSlot(null);
        }

        // Handles dragging an attachment onto a weapon in the Inventory and its compatibility check & assignment.
        // Only Inventory / Chest is handled here. Hotbar is handled separately.
        private bool DragAttachmentOnWeaponSlot(bool clearCurrentReferences)
        {
            InventorySlot draggedAnchor = draggedSlot.GetAnchorSlot();
            InventorySlot highlightedAnchor = highlightedSlot.GetAnchorSlot();
            bool canAttachOnWeapon = IsValidAttachmentWeaponPair(draggedAnchor, highlightedAnchor);

            if (canAttachOnWeapon)
            {
                // First check if the attachment is compatible with the target weapon
                (bool success, Attachment atc, int id) =
                    CowsinsUtilities.CompatibleAttachment((Weapon_SO)highlightedAnchor.slotData.item, (AttachmentIdentifier_SO)draggedAnchor.slotData.item);

                // If compatible, assign the att    achment.
                if (success)
                {
                    ToastManager.Instance.ShowToast($"{draggedAnchor.slotData.item._name} {ToastManager.Instance.AttachmentSuccessfullyAddedMsg} {highlightedAnchor.slotData.item._name}");
                    SoundManager.Instance.PlaySound(placeItemSFX, 0, 0, false, 0);

                    gridGenerator.ClearSlotArea(draggedAnchor);
                    ResetHighlightedSlotsArea(draggedSlot.DefaultBackgroundColor, clearCurrentReferences);

                    switch (atc)
                    {
                        case Barrel barrel:
                            SwapAttachmentOnWeapon(highlightedAnchor, ref highlightedAnchor.slotData.barrel, barrel);
                            break;
                        case Scope scope:
                            SwapAttachmentOnWeapon(highlightedAnchor, ref highlightedAnchor.slotData.scope, scope);
                            break;
                        case Stock stock:
                            SwapAttachmentOnWeapon(highlightedAnchor, ref highlightedAnchor.slotData.stock, stock);
                            break;
                        case Grip grip:
                            SwapAttachmentOnWeapon(highlightedAnchor, ref highlightedAnchor.slotData.grip, grip);
                            break;
                        case Magazine magazine:
                            SwapAttachmentOnWeapon(highlightedAnchor, ref highlightedAnchor.slotData.magazine, magazine);
                            break;
                        case Flashlight flashlight:
                            SwapAttachmentOnWeapon(highlightedAnchor, ref highlightedAnchor.slotData.flashlight, flashlight);
                            break;
                        case Laser laser:
                            SwapAttachmentOnWeapon(highlightedAnchor, ref highlightedAnchor.slotData.laser, laser);
                            break;
                    }

                    // Handle Attachments for Hotbars, so they are immediately assigned after dropping the attachment on player.
                    if (highlightedAnchor && highlightedAnchor.IsHotbarSlot)
                    {
                        SlotData highlightedSlotData = highlightedAnchor.slotData;
                        int colId = highlightedAnchor.col;
                        var attachments = new[] { highlightedSlotData.barrel, highlightedSlotData.scope, highlightedSlotData.stock, highlightedSlotData.grip,
                        highlightedSlotData.magazine, highlightedSlotData.flashlight, highlightedSlotData.laser };
                        foreach (var attachment in attachments)
                        {
                            (Attachment newAtc, int newId) = CowsinsUtilities.GetAttachmentID(attachment, weaponController.inventory[colId]);
                            weaponController.AssignAttachmentToWeapon(newAtc, newId, colId);
                        }

                        // Unholster the weapon so we can actually use it.
                        if (weaponController.currentWeapon == colId) weaponController.SelectWeapon();
                    }

                    return true;
                }
            }
            return false;
        }

        private void SwapAttachmentOnWeapon<T>(InventorySlot highlightedAnchor, ref AttachmentIdentifier_SO atcIdentifier, T attachment) where T : Attachment
        {
#if INVENTORY_PRO_ADD_ON
            // If there is already an attachment in place, we�ll need to either add it to the Inventory or drop it.
            if (!highlightedAnchor.IsHotbarSlot && atcIdentifier != null)
                // interactManager.TryAddAttachmentToInventory(atcIdentifier); #MYTODO
#endif
            // After the old attachment has been handled, assign the new attachment
            atcIdentifier = attachment.attachmentIdentifier;
        }

        private bool CanAttachDraggedAttachmentToWeaponSlot()
        {
            InventorySlot draggedAnchor = draggedSlot?.GetAnchorSlot();
            InventorySlot highlightedAnchor = highlightedSlot?.GetAnchorSlot();
            bool canAttachOnWeapon = IsValidAttachmentWeaponPair(draggedAnchor, highlightedAnchor);

            if (canAttachOnWeapon)
            {
                (bool success, Attachment atc, int id) =
                    CowsinsUtilities.CompatibleAttachment((Weapon_SO)highlightedAnchor.slotData.item, (AttachmentIdentifier_SO)draggedAnchor.slotData.item);
                return success;
            }

            return false;
        }

        private bool IsValidAttachmentWeaponPair(InventorySlot draggedAnchor, InventorySlot highlightedAnchor)
        {
            bool canAttachOnWeapon = draggedAnchor && highlightedAnchor && draggedAnchor.HasValidItem && draggedAnchor.slotData.item is AttachmentIdentifier_SO
                && highlightedAnchor.HasValidItem && highlightedAnchor.slotData.item is Weapon_SO;
            return canAttachOnWeapon;
        }

        /// <summary>
        /// Handles moving an Item from the Hotbar to the Inventory
        /// </summary>
        public void SwapFromHotbarToInventory()
        {
            if (!IsValidPosition(highlightedSlot, draggedSlot.GetItemSize()) || highlightedSlot.anchorSlot != null) return;

            // Sets Inventory Slot Data to Hotbar�s
            highlightedSlot.slotData = draggedSlot.slotData;
            // Reset Hotbar Slot Data
            draggedSlot.slotData = new SlotData();

            draggedSlot.anchorSlot = null;
            highlightedSlot.anchorSlot = highlightedSlot;

            if (useFavouritesRadialMenu) favItemsMenu.UpdatePinnedFavorites(draggedSlot.GetAnchorSlot(), highlightedSlot.GetAnchorSlot());

            // Since the Hotbar Slots are necessarily 1x1 slots, simply update its graphics
            draggedSlot.UpdateSlotGraphics();
            // If using Tetris Inventory, inventory Slots can take up to different sizes, so we cannot just call UpdateSlotGraphics, but ReassignSlots
            ReassignSlots(highlightedSlot);

            // Store bullets into the target slot data
            int currentBullets = weaponController.inventory[draggedSlot.col].bulletsLeftInMagazine;
            int totalBullets = weaponController.inventory[draggedSlot.col].totalBullets;
            highlightedSlot.slotData.bulletsLeftInMagazine = currentBullets;
            highlightedSlot.slotData.totalBullets = totalBullets;

            // Ensure we clear the Weapon Controller inventory 
            if (draggedSlot.col == weaponController.currentWeapon) weaponController.ReleaseCurrentWeapon();
            else if (weaponController.inventory[draggedSlot.col] != null)
            {
                Destroy(weaponController.inventory[draggedSlot.col].gameObject);
                weaponController.weaponsInventoryUISlots[draggedSlot.col].SetWeapon(null);
            }
            SoundManager.Instance.PlaySound(placeItemSFX, 0, 0, false, 0);
        }

        /// <summary>
        /// Handles moving an Item from the Inventory to the Hotbar.
        /// Only weapons can be moved to the Hotbar
        /// </summary>
        public void SwapFromInventoryToHotbar()
        {
            if (!draggedSlot.IsItemWeapon || highlightedSlot.HasValidItem) return;

            // Update hotbar slot data & visuals
            highlightedSlot.slotData = draggedSlot.anchorSlot.slotData;
            highlightedSlot.UpdateSlotGraphics();

            // Clear Inventory Slots since the data has been moved
            gridGenerator.ClearSlotArea(draggedSlot);

            // Store the column id of the highlighted slot
            int colId = highlightedSlot.col;
            if (weaponController.currentWeapon == colId && weaponController.inventory[weaponController.currentWeapon] != null) weaponController.ReleaseCurrentWeapon();

            // Handle Instantiating a new Weapon if we move the Weapon to the current hotbar slot.
            Weapon_SO cachedHighlightedWeapon = (Weapon_SO)highlightedSlot.slotData.item;
            weaponController.weaponsInventoryUISlots[colId].SetWeapon(cachedHighlightedWeapon);

            // Ensure Fav Item is up-to-date if applicable
            if (useFavouritesRadialMenu) favItemsMenu.UpdatePinnedFavorites(draggedSlot.GetAnchorSlot(), highlightedSlot.GetAnchorSlot());

            if (cachedHighlightedWeapon == null) return;
            
            weaponController.InstantiateWeapon(cachedHighlightedWeapon, colId, highlightedSlot.slotData.bulletsLeftInMagazine, highlightedSlot.slotData.totalBullets);

            SlotData highlightedSlotData = highlightedSlot.slotData;

            // Handle Attachments
            var attachments = new[] { highlightedSlotData.barrel, highlightedSlotData.scope, highlightedSlotData.stock, highlightedSlotData.grip,
            highlightedSlotData.magazine, highlightedSlotData.flashlight, highlightedSlotData.laser };
            foreach (var attachment in attachments)
            {
                (Attachment atc, int id) = CowsinsUtilities.GetAttachmentID(attachment, weaponController.inventory[colId]);
                weaponController.AssignAttachmentToWeapon(atc, id, colId);
            }

            // Unholster the weapon so we can actually use it.
            if (weaponController.currentWeapon == colId) weaponController.SelectWeapon();

            SoundManager.Instance.PlaySound(placeItemSFX, 0, 0, false, 0);
        }

        /// <summary>
        /// Combines the passed item with an existing stack in the specified inventory slot.
        /// If the slot contains the same item, their quantities are merged up to the item's stack limit.
        /// </summary>
        /// <param name="slot">The inventory slot containing the item stack to be updated.</param>
        public void StackItems(InventorySlot slot, bool playSFX)
        {
            Item_SO highlightedItem = highlightedSlot.slotData.item;
            // stack if possible
            int stackableAmount = highlightedItem.maxStack - highlightedSlot.slotData.amount;
            int amountToMove = Mathf.Min(slot.slotData.amount, stackableAmount); // How many items to move
            Vector2Int itemSize = slot.GetItemSize();

            // Add items to the highlighted slot
            highlightedSlot.slotData.amount += amountToMove;

            // Subtract the moved amount from the current slot
            slot.slotData.amount -= amountToMove;
            if (slot.slotData.amount <= 0)
                gridGenerator.ClearSlotArea(slot);
            else ReassignSlots(slot);

            // Reassign the items in the highlighted slot
            ReassignSlots(highlightedSlot);
            if (useFavouritesRadialMenu) favItemsMenu.UpdatePinnedFavorites(draggedSlot.GetAnchorSlot(), highlightedSlot.GetAnchorSlot());

            events.onStackItems?.Invoke();
            if(playSFX) SoundManager.Instance.PlaySound(placeItemSFX, 0, 0, false, 0);
        }

        /// <summary>
        /// Swaps the item in the current inventory slot with the item in the specified target slot.
        /// If either slot is empty, the item is simply moved to the other slot.
        /// </summary>
        /// <param name="targetSlot">The inventory slot to swap items with.</param>
        public void SwapItems(InventorySlot targetSlot, bool playSFX)
        {
            // Check if it's a hotbar slot; handle simple swap if either slot is a hotbar slot
            // Get the sizes of the dragged item and the target item
            Vector2Int draggedItemSize = draggedSlot.GetAnchorSlot().GetItemSize();
            Vector2Int targetItemSize = targetSlot && targetSlot.anchorSlot != null && targetSlot.HasValidItem
                ? targetSlot.anchorSlot.GetItemSize() : new Vector2Int(1, 1);
            if ((draggedItemSize != targetItemSize && draggedSlot?.anchorSlot != null && targetSlot?.anchorSlot != null && draggedSlot?.anchorSlot != targetSlot?.anchorSlot)
                || !IsValidPosition(targetSlot, draggedItemSize))
            {
                return;
            }
            targetSlot = targetSlot.GetAnchorSlot();

            // Store temporary references of the slot data
            InventorySlot tempDraggedAnchor = draggedSlot.GetAnchorSlot();
            InventorySlot tempTargetAnchor = targetSlot.GetAnchorSlot();
            SlotData tempDraggedData = draggedSlot.GetAnchorSlot().slotData;
            SlotData tempTargetData = targetSlot.slotData;

            // Check if pinned item needs to be updated, & update it accordingly to keep track of Fav Items & avoid missmatches
            if (useFavouritesRadialMenu) favItemsMenu.UpdatePinnedFavorites(tempDraggedAnchor, tempTargetAnchor);
            gridGenerator.ClearSlotArea(draggedSlot);

            if (tempDraggedAnchor != tempTargetAnchor)
            {
                // Swap the slot data and anchors
                draggedSlot.anchorSlot = tempDraggedAnchor;
                draggedSlot.anchorSlot.slotData = tempTargetData;
                targetSlot.slotData = tempDraggedData;

                // Update anchor references
                draggedSlot.anchorSlot = draggedSlot.HasValidItem ? draggedSlot : null;
                targetSlot.anchorSlot = targetSlot.HasValidItem ? targetSlot : null;

                ReassignSlots(draggedSlot);
                ReassignSlots(targetSlot);
            }
            else
            {
                highlightedSlot.anchorSlot = highlightedSlot;
                highlightedSlot.anchorSlot.slotData = tempDraggedData;

                // Re-assign slot anchors for dragged and target items
                ReassignSlots(highlightedSlot);

                if (useFavouritesRadialMenu) favItemsMenu.UpdatePinnedFavorites(draggedSlot.GetAnchorSlot(), highlightedSlot.GetAnchorSlot());
            }
            if (playSFX) SoundManager.Instance.PlaySound(placeItemSFX, 0, 0, false, 0);
        }

        public void UpdateBulletsHotbar()
        {
            bool isDraggedItemWeapon = draggedSlot ? draggedSlot.IsItemWeapon : false;
            bool isHighlightedItemWeapon = highlightedSlot ? highlightedSlot.IsItemWeapon : false;

            if (isDraggedItemWeapon && draggedSlot.IsHotbarSlot)
            {
                int draggedCol = draggedSlot.col;
                draggedSlot.slotData.bulletsLeftInMagazine = weaponController.inventory[draggedCol].bulletsLeftInMagazine;
                draggedSlot.slotData.totalBullets = weaponController.inventory[draggedCol].totalBullets;
            }
            if (isHighlightedItemWeapon && highlightedSlot.IsHotbarSlot)
            {
                int highlightedCol = highlightedSlot.col;
                highlightedSlot.slotData.bulletsLeftInMagazine = weaponController.inventory[highlightedCol].bulletsLeftInMagazine;
                highlightedSlot.slotData.totalBullets = weaponController.inventory[highlightedCol].totalBullets;
            }
        }

        /// <summary>
        /// Swaps the item in the current Hotbar slot with the item in the specified target slot.
        /// If either slot is empty, the item is simply moved to the other slot.
        /// </summary>
        public void SwapHotbarItems()
        {
            // Stop reloading if we are reloading & we moved the Weapon to avoid unexpected errors
            if (weaponController.Reloading &&
                (weaponController.id == weaponController.inventory[draggedSlot.col] || weaponController.id == weaponController.inventory[highlightedSlot.col]))
                weaponController.StopReload();

            // Swap Slot Datas
            SlotData cacheDraggedData = draggedSlot.slotData;
            draggedSlot.slotData = highlightedSlot.slotData;
            highlightedSlot.slotData = cacheDraggedData;

            // Check if pinned item needs to be updated, & update it accordingly to keep track of Fav Items & avoid missmatches
            if (useFavouritesRadialMenu) favItemsMenu.UpdatePinnedFavorites(draggedSlot.GetAnchorSlot(), highlightedSlot.GetAnchorSlot());

            draggedSlot.anchorSlot = null;
            highlightedSlot.anchorSlot = null;

            // Update Visuals
            draggedSlot.UpdateSlotGraphics();
            highlightedSlot.UpdateSlotGraphics();

            HandleHotbarEndDrag();

            SoundManager.Instance.PlaySound(placeItemSFX, 0, 0, false, 0);
        }

        public void HandleHotbarEndDrag()
        {
            // Check if dragged slot is hotbar and highlighted is not
            if (draggedSlot.IsHotbarSlot && !highlightedSlot.IsHotbarSlot && highlightedSlot.HasValidItem)
            {
                if (weaponController.inventory[weaponController.currentWeapon])
                    weaponController.ReleaseCurrentWeapon();
            }
            // Check if both dragged and highlighted slots are hotbar
            else if (draggedSlot.IsHotbarSlot && highlightedSlot.IsHotbarSlot)
            {
                int draggedCol = draggedSlot.col;
                int highlightedCol = highlightedSlot.col;
                WeaponIdentification saveSlot = weaponController.inventory[draggedCol];
                weaponController.inventory[draggedCol] = weaponController.inventory[highlightedCol];
                weaponController.inventory[highlightedCol] = saveSlot;

                weaponController.weaponsInventoryUISlots[draggedCol].SetWeapon(weaponController.inventory[draggedCol]?.weapon);

                weaponController.weapon = null;
                weaponController.id = null;

                weaponController.SelectWeapon();
            }
        }

        /// <summary>
        /// Detects if a dragged item was dropped outside the boundaries of the Inventory, Chest or Delete Area and spawns a pickeable, as it means the item was dropped.
        /// </summary>
        /// <param name="anchorSlot"></param>
        /// <returns></returns>
        public bool HandleDropOutsideAndItemDeletion(InventorySlot anchorSlot)
        {
            if (highlightedSlot != null) return false;

            RectTransform inventoryRectTransform = _GridGenerator.GetComponent<RectTransform>();
            RectTransform trashRectTransform = TrashButton;
            RectTransform chestRectTransform = _GridGenerator.ChestParent.GetComponent<RectTransform>();

            Vector2 mousePosition = Mouse.current.position.ReadValue();

            // Check if the drop happened outside the inventory bounds
            if (!RectTransformUtility.RectangleContainsScreenPoint(inventoryRectTransform, mousePosition))
            {
                // Check if the item was dropped on the trash button
                if (RectTransformUtility.RectangleContainsScreenPoint(trashRectTransform, mousePosition))
                {
                    // Delete the dragged item
                    DeleteItem(anchorSlot);
                    draggedSlot = null;
                    return true;
                }
                if (!RectTransformUtility.RectangleContainsScreenPoint(chestRectTransform, mousePosition))
                {
                    // Drop the item outside the inventory (current behavior)
                    DropOutsideInventory(anchorSlot);
                    draggedSlot = null;
                    return true;
                }

                DropOutsideInventory(anchorSlot);
                draggedSlot = null;
                return true;
            }

            return false;
        }
        public void LootAllChest()
        {
            for (int i = 0; i < ChestSlots.GetLength(0); i++)
            {
                for (int j = 0; j < ChestSlots.GetLength(1); j++)
                {
                    draggedSlot = ChestSlots[i, j]?.GetAnchorSlot();
                    Item_SO item = draggedSlot.slotData.item;

                    if (item == null) continue;

                    // Gather whether we want to move from chest to inventory or vice versa
                    // We�ll need to find an available slot to move the item, and swap or stack them
                    InventorySlot targetSlot = FindAvailableSlotInventory(item);

                    if (targetSlot?.slotData?.item == null) SwapItems(targetSlot, playSFX: false);
                    else
                    {
                        HighlightSlot(targetSlot);
                        StackItems(draggedSlot, playSFX: false);
                    }
                }
            }
            ClearDraggedHighlightedReferences();
        }

        public void MoveItemTo(InventorySlot currentSlot, InventorySlot targetSlot)
        {
            draggedSlot = currentSlot?.GetAnchorSlot();
            draggedSlot.anchorSlot = currentSlot;

            if (targetSlot?.slotData?.item == null) SwapItems(targetSlot, playSFX: true);
            else
            {
                HighlightSlot(targetSlot);
                StackItems(draggedSlot, playSFX: true);
            }
            ClearDraggedHighlightedReferences();
        }
        /// <summary>
        /// Returns whether the passed SlotData can be stacked.
        /// </summary>
        public bool CanStackItems(SlotData slotData)
        {
            Item_SO draggedItem = slotData.item;
            Item_SO highlightedItem = highlightedSlot.slotData.item;

            if (draggedItem?.maxStack == 1 || highlightedItem?.maxStack == 1) return false;

            return draggedItem && highlightedItem && draggedItem.GetType() == highlightedItem.GetType() && draggedItem?._name == highlightedItem._name && this != highlightedSlot;
        }

        public bool PreventEndDrag()
        {
            return draggedSlot == null || !draggedSlot.HasValidItem;
        }

        /// <summary>
        /// Enables Drag Icon and disables tooltip when we start to drag an Item
        /// </summary>
        /// <param name="item"></param>
        public void StartDragVisuals(Item_SO item)
        {
            _TooltipManager.SetDragImageVisibility(true, item.icon, inventoryStyle == InventoryStyle.Tetris && DeviceDetection.Instance.mode == DeviceDetection.InputMode.Keyboard);
            _TooltipManager.SetTooltipVisibility(false, "", "");
            SetAreaColorOnPointerEnter(draggedSlot);
        }

        public void OnBeginDrag(InventorySlot selectedAnchorSlot, bool allowBeginDrag)
        {
            // Force rotation on the new dragged Item
            RotateItem(itemRotation);

            if (allowBeginDrag)
            {
                draggedSlot = selectedAnchorSlot;
                StartDragVisuals(selectedAnchorSlot.slotData.item);
            }

            itemRotation = draggedSlot?.slotData != null && draggedSlot.slotData.isOriented ? 0 : 90;
            _TooltipManager.Rotate(itemRotation);

            events.onSlotBeginDrag?.Invoke();
        }

        /// <summary>
        /// Called when hovering the given slot
        /// </summary>
        /// <param name="slot">InventorySlot to highlight</param>
        public void OnPointerEnter(InventorySlot slot)
        {
            HighlightSlot(slot);
            contextMenu.HideContextMenu();
            if (!draggedSlot || !draggedSlot.HasValidItem) return;
            SetAreaColorOnPointerEnter(slot);
        }
        /// <summary>
        /// Called when unhovering the given slot
        /// </summary>
        /// <param name="slot">InventorySlot to unhighlight</param>
        public void OnPointerExit(Color defaultBackgroundColor)
        {
            HighlightSlot(null);
            _TooltipManager.SetTooltipVisibility(false, "", "");
            SetSlotsColors(defaultBackgroundColor);
            highlightedSlotArea = new List<InventorySlot>();
        }

        /// <summary>
        /// Called when a Drag & Drop Action is finished.
        /// </summary>
        public void OnEndDrag(bool clearCurrentReferences)
        {
            // Disable Tooltip & Drag Image
            _TooltipManager.SetDragImageVisibility(false, null, false);
            events.onSlotEndDrag?.Invoke();

            if (PreventEndDrag())
            {
                ClearDraggedHighlightedReferences();
                return;
            }

            // First check if the Item was dropped outside the boundaries of the Inventory & process its deletion
            bool isItemDropped = HandleDropOutsideAndItemDeletion(draggedSlot.GetAnchorSlot());
            UpdateBulletsHotbar();

            if (isItemDropped || highlightedSlot == null)
            {
                ClearDraggedHighlightedReferences();
                return;
            }

            // Then check if we dropped an attachment onto a Weapon
            if (DragAttachmentOnWeaponSlot(clearCurrentReferences)) return;

            // If none of the previous scenarios occured, then we�ll proceed to determine how to update the slots.

            bool isDraggedHotbar = draggedSlot.IsHotbarSlot;
            bool isHighlightedHotbar = highlightedSlot.IsHotbarSlot;
            bool isDraggedChest = draggedSlot.IsChestSlot;
            bool isHighlightedChest = highlightedSlot.IsChestSlot;
            bool canStack = CanStackItems(draggedSlot.slotData);
            InventorySlot hotbarSlot = isDraggedHotbar ? draggedSlot :
               (isHighlightedHotbar ? highlightedSlot : null);

            if (isDraggedHotbar != isHighlightedHotbar) // Hotbar <-> Inventory swap
            {
                if (weaponController.Reloading && weaponController.id == weaponController.inventory[hotbarSlot.col])
                    weaponController.StopReload();

                if (isDraggedHotbar) SwapFromHotbarToInventory();
                else SwapFromInventoryToHotbar();
            }
            else if (draggedSlot.IsInventorySlot || draggedSlot.IsChestSlot)
            {
                if (canStack) StackItems(draggedSlot, playSFX: true);
                else SwapItems(highlightedSlot, playSFX: true);
            }
            else // Hotbar swaps
            {
                SwapHotbarItems();
            }

            ResetHighlightedSlotsArea(draggedSlot.DefaultBackgroundColor, clearCurrentReferences);
        }
        /// <summary>
        /// Returns an InventorySlot in the Hotbar that is empty.
        /// </summary>
        public InventorySlot FindAvailableSlotHotbar()
        {
            foreach (var slot in HotbarSlots)
            {
                if (slot.slotData.item == null || slot.slotData == null)
                    return slot;
            }
            return null;
        }

        /// <summary>
        /// Returns an InventorySlot in the Inventory that is available for the given Item to be stored.
        /// </summary>
        public InventorySlot FindAvailableSlotInventory(Item_SO item) { return FindAvailableSlot(item, InventorySlots); }

        /// <summary>
        /// Returns an InventorySlot in the Chest that is available for the given Item to be stored.
        /// </summary>
        public InventorySlot FindAvailableSlotChest(Item_SO item) { return FindAvailableSlot(item, ChestSlots); }

        private InventorySlot FindAvailableSlot(Item_SO item, InventorySlot[,] array)
        {
            foreach (var slot in array)
            {
                if (slot.slotData.item == null && IsValidPosition(slot, item.itemSize) || slot.slotData?.item == item && slot.slotData?.amount < slot.slotData?.item.maxStack)
                    return slot;
            }
            return null;
        }

        /// <summary>
        /// Returns whether the style of the Inventory is Grid or Tetris. True if Grid is returned.
        /// </summary>
        public bool IsGridInventory() { return inventoryStyle == InventoryStyle.Grid; }

        private void OnAddedItem(Item_SO item, int amount = 1)
        {
            if (item is null)
            {
                Debug.Log("item is null when adding");
                return;
            }
            weightSystem.CanPickUp(item.Weight, amount);
        }

        private void OnRemoveItem(Item_SO item, int amount = 1)
        {
            if (item is null)
            {
                Debug.Log("item is null when removing");
                return;
            }
            weightSystem.RemoveItems(item.Weight, amount);
        }

#if UNITY_EDITOR
        public void SetRightClickEvent(SlotRightClickEvent slotRightClickEvent) => this.slotRightClickEvent = slotRightClickEvent;
#endif
    }
}