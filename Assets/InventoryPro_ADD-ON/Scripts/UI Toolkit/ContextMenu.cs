using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cowsins.Inventory
{
    /// <summary>
    /// You can Right click on an Inventory, Hotbar or Chest Slot to open the ContextMenu.
    /// The Context Menu contains different Actions that you can perform based on the Highlighted Slot.
    /// </summary>
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public class ContextMenu : MonoBehaviour
    {
        [SerializeField, Title("CONTEXT MENU REFERENCES")] private InventoryProManager inventoryProManager;
        [SerializeField] private Button useButton, pinButton, unpinButton, dropButton, splitButton, moveToButton, lootAllChestButton;
        
        // INTERNAL USE
        private InventorySlot currentSlot;
        private CanvasGroup canvasGroup;
        private AudioClip clickSFX;
        private Image image;

        private void Awake()
        {
            // Gather references
            canvasGroup = GetComponent<CanvasGroup>();
            image = GetComponent<Image>();

            // Add Buttons Listeners so they can perform their respective Actions on being clicked.
            useButton.onClick.AddListener(UseItem);
            dropButton.onClick.AddListener(DropItem);
            splitButton.onClick.AddListener(SplitItemCount);
            if (inventoryProManager.UseFavouritesRadialMenu)
            {
                pinButton.onClick.AddListener(PinToFavorite);
                unpinButton.onClick.AddListener(UnpinFromFavorites);
            }

            clickSFX = inventoryProManager.ContextMenuClickSFX;

            // Ensure Context Menu is disabled ( hidden ) by default
            HideContextMenu();
        }

        private void OnDestroy()
        {
            // Disable Buttons functionality
            useButton.onClick.RemoveListener(UseItem);
            dropButton.onClick.RemoveListener(DropItem);
            splitButton.onClick.RemoveListener(SplitItemCount);
            if (inventoryProManager.UseFavouritesRadialMenu)
                pinButton.onClick.RemoveListener(PinToFavorite);
        }

        public void ShowContextMenu(InventorySlot slot, Vector3 position, float raycastPadding)
        {
            // Avoid showing the context menu if the slot is null or if it does not contain an item.
            if (slot == null || !slot.HasValidItem)
            {
                HideContextMenu();
                return;
            }

            // Adjust visibility + position
            transform.position = position;
            inventoryProManager._TooltipManager.SetTooltipVisibility(false, string.Empty, string.Empty);

            currentSlot = slot;

            // Make it visible
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            image.raycastPadding = Vector4.one * -raycastPadding;

            // Check if the "Use" method is overridden
            Item_SO item = currentSlot.slotData.item;

            // To avoid Import issues, this code is only compiled if Inventory Pro Add-On is properly installed.
#if INVENTORY_PRO_ADD_ON
            var useMethod = item.GetType().GetMethod(nameof(item.Use));
            var baseMethod = typeof(Item_SO).GetMethod(nameof(item.Use));
            bool isChestSlot = slot.IsChestSlot;

            bool isOverridden = useMethod.DeclaringType != baseMethod.DeclaringType;

            // We can only pin to favourites items that declare "Use" Actions
            FavItemReference newFavItemReference = new FavItemReference(
                row: slot.row,
                col: slot.col,
                item: slot.slotData.item,   
                isInventorySlot: slot.IsInventorySlot);

            bool isPinned = inventoryProManager._FavItemsMenu.ContainsItemReferenceInPinned(newFavItemReference).Item1;

            if(isPinned)
            {
                unpinButton.gameObject.SetActive(true);
                pinButton.gameObject.SetActive(false);
            }
            else
            {
                unpinButton.gameObject.SetActive(false);
                pinButton.gameObject.SetActive(inventoryProManager.UseFavouritesRadialMenu && isOverridden && !isChestSlot);
            }

            // We can only use the item if "Use" is overridden and this is not a chest
            if(useButton)
            {
                useButton.gameObject.SetActive(isOverridden && !isChestSlot);
            }

            // Split button can only appear if amount is greater than 1
            if(splitButton)
            {
                bool hasMultipleAmount = slot.slotData.amount > 1;
                splitButton.gameObject.SetActive(hasMultipleAmount && !currentSlot.IsHotbarSlot);
            }


            // Move Button Text will vary based on whether the slot is a chest slot or an Inventory Slot
            if(moveToButton)
            {
                bool isChestOpen = inventoryProManager.ChestSlots != null && inventoryProManager.ChestSlots.Length > 0;
                bool isChestFull = inventoryProManager._GridGenerator.IsChestFull();
                moveToButton.gameObject.SetActive(isChestOpen && !isChestFull && !currentSlot.IsHotbarSlot);
                moveToButton.GetComponentInChildren<TextMeshProUGUI>().text = isChestSlot ? "MOVE TO INVENTORY" : "MOVE TO CHEST";
                moveToButton.onClick.RemoveAllListeners();
                moveToButton.onClick.AddListener(() => MoveItemTo(isChestSlot));
            }

            if(lootAllChestButton)
            {
                lootAllChestButton.gameObject.SetActive(currentSlot.IsChestSlot);
                lootAllChestButton.onClick.RemoveAllListeners();
                lootAllChestButton.onClick.AddListener(() => LootAllChest());
            }
#endif
        }

        /// <summary>
        /// Disables Context Menu from UI.
        /// </summary>
        public void HideContextMenu()
        {
            transform.position = Vector3.zero;
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0;
            currentSlot = null;
        }

        private void UseItem()
        {
            // Avoid calling the use method wrong if the currentSLot is null
            if (currentSlot == null)
            {
                HideContextMenu();
                return;
            }
            ToastManager.Instance.ShowToast($"{currentSlot.slotData.item._name} {ToastManager.Instance.ItemUsed}");
#if INVENTORY_PRO_ADD_ON
            currentSlot.slotData.item.Use(inventoryProManager, currentSlot);
#endif
            SoundManager.Instance.PlaySound(clickSFX, 0, 0, false, 0);
            HideContextMenu();
        }

        private void DropItem()
        {
            // Avoid calling the drop method wrong if the currentSLot is null
            if (currentSlot == null)
            {
                HideContextMenu();
                return;
            }

            inventoryProManager.DropOutsideInventory(currentSlot);
            SoundManager.Instance.PlaySound(clickSFX, 0, 0, false, 0);
            HideContextMenu();
        }

        private void SplitItemCount()
        {
            // Avoid calling the split method wrong if the currentSLot is null
            inventoryProManager.SplitOutsideInventory(currentSlot.GetAnchorSlot());
            SoundManager.Instance.PlaySound(clickSFX, 0, 0, false, 0);
            HideContextMenu();
        }

        private void MoveItemTo(bool isChestSlot)
        {
            Item_SO item = currentSlot.slotData.item;

            // Gather whether we want to move from chest to inventory or vice versa
            // We´ll need to find an available slot to move the item, and swap or stack them
            InventorySlot targetSlot = isChestSlot ? inventoryProManager.FindAvailableSlotInventory(item) : inventoryProManager.FindAvailableSlotChest(item);
            inventoryProManager.MoveItemTo(currentSlot, targetSlot);
            SoundManager.Instance.PlaySound(clickSFX, 0, 0, false, 0);
            HideContextMenu();
        }

        private void PinToFavorite()
        {
            // Avoid calling the pin method wrong if the currentSLot is null
            if (currentSlot == null)
            {
                HideContextMenu();
                return;
            }
            // We´ll pin the anchor/origin of the slot (Just in case we are using a Tetris inventory)
            InventorySlot anchor = currentSlot.GetAnchorSlot();

            FavItemReference favItemReference = new FavItemReference(
                anchor.row, anchor.col, anchor.slotData.item, anchor.IsInventorySlot);

            if (inventoryProManager._FavItemsMenu.ContainsItemReferenceInPinned(favItemReference).Item1)
            {
                ToastManager.Instance?.ShowToast(ToastManager.Instance.AlreadyPinnedToastMsg);
                SoundManager.Instance.PlaySound(clickSFX, 0, 0, false, 0);
                HideContextMenu();
                return;
            }

            ToastManager.Instance?.ShowToast(!inventoryProManager._FavItemsMenu.AddItemReferenceToPinned(favItemReference) ?
                ToastManager.Instance.FavPinnedMenuIsFullMsg : $"{favItemReference.item.name}{ToastManager.Instance.ItemPinnedToastMsg}");

            SoundManager.Instance.PlaySound(clickSFX, 0, 0, false, 0);
            HideContextMenu();
        }

        private void UnpinFromFavorites()
        {
            if (currentSlot == null)
            {
                HideContextMenu();
                return;
            }
            InventorySlot anchor = currentSlot.GetAnchorSlot();

            FavItemReference favItemReference = new FavItemReference(
                anchor.row, anchor.col, anchor.slotData.item, anchor.IsInventorySlot);
            if (inventoryProManager._FavItemsMenu.ContainsItemReferenceInPinned(favItemReference).Item1)
            {
                inventoryProManager._FavItemsMenu.UnpinFromFavMenu(anchor);
                SoundManager.Instance.PlaySound(clickSFX, 0, 0, false, 0); 
                ToastManager.Instance?.ShowToast($"{favItemReference.item.name}{ToastManager.Instance.ItemUnpinnedToastMsg}");
                HideContextMenu();
                return;
            }
        }

        private void LootAllChest()
        {
            inventoryProManager.LootAllChest();
            SoundManager.Instance.PlaySound(clickSFX, 0, 0, false, 0);
            HideContextMenu();
        }
    }
}