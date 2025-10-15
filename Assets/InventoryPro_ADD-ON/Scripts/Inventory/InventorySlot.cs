using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Rendering;

namespace cowsins.Inventory
{
    /// <summary>
    /// In InventoryProManager there are 3 types of Slots: Inventory, Hotbar, Chest
    /// </summary>
    [System.Serializable]
    public enum SlotType
    {
        Inventory, Hotbar, Chest
    }

    /// <summary>
    /// When using the Inventory or Chests in FPS Engine, Players will interact with InventorySlots. InventorySlot contains information about items, quantities & handles
    /// interactions between the InventoryProManager and the Player.
    /// </summary>
    public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        #region variables
        // READ-ONLY PROPERTIES FOR DEBUGGING
        [ReadOnly] public int row;
        [ReadOnly] public int col;
        [ReadOnly] public InventorySlot anchorSlot;
        [ReadOnly] public SlotData slotData;
        [ReadOnly] public SlotType slotType;

        [SerializeField, Tooltip("Image that dipslays the Icon of the Item held by this InventorySlot")] private Image iconImage;
        [SerializeField, Tooltip("Text that shows the current amount or quantity stored in the slot data of this inventory slot.")] private TextMeshProUGUI amountText;
        [SerializeField, Tooltip("Background of the Slot. This is coloured to indicate if the Slot is Available or " +
            "occupied when hovered while dragging an object.")] private Image backgroundImage;

        [Title("Inventory Slot Interactions")]
        [SerializeField, Tooltip("Inventory Slot object Scale when hovered/highlighted")] private float highlightedScale = 1.2f;
        [SerializeField, Tooltip("GameObject that gets enabled when the InventorySlot is highlighted. This particularly simulates a border.")] private GameObject highlightedBorder;
        [SerializeField, Tooltip("Speed to lerp from the default scale to the highlightedScale")] private float lerpSpeed = 5f;
        [SerializeField, Tooltip("Adjusts Icon Image Spacing to properly fit the Icons in the Slot")] private Vector3 iconImageMargins = new Vector3(10, -10, 0);
        [SerializeField, Tooltip("Object that contains the entire Inventory Slot")] private Transform inventoryContainer;

        // INTERNAL USE
        private Vector3 normalScale;
        protected InventoryProManager inventoryManager;
        protected WeaponController weaponController;
        protected SortingGroup sortingGroup;
        private float timeToShowTooltip = .4f;
        private RectTransform iconRect;
        protected Color defaultBackgroundColor;

        // GETTERS
        public Color DefaultBackgroundColor => defaultBackgroundColor;
        public Image BackgroundImage => backgroundImage;

        public bool IsHotbarSlot => slotType == SlotType.Hotbar;

        public bool IsInventorySlot => slotType == SlotType.Inventory;

        public bool IsChestSlot => slotType == SlotType.Chest;

        public SlotType SlotType => slotType;

        public bool HasValidItem => slotData != null && slotData.item != null && slotData.amount > 0;

        public string ItemName => slotData?.item != null ? slotData.item._name : "";

        public Sprite ItemIcon => IsHotbarSlot || GetItemSize() == new Vector2Int(1, 1) ? slotData?.item?.icon :
                (slotData.item.irregularItemIcon != null ? slotData.item.irregularItemIcon : slotData.item.icon);

        public string ItemDescription => slotData?.item != null ? slotData.item.description : "";

        public bool IsItemWeapon => slotData?.item is Weapon_SO;

        public bool IsItemAttachment => slotData?.item is AttachmentIdentifier_SO;

        public bool IsItemBullets => slotData?.item is BulletsItem_SO;

        #endregion

        #region initialization 

        private void Awake()
        {
            normalScale = transform.localScale;
            iconRect = iconImage.GetComponent<RectTransform>();
            sortingGroup = GetComponent<SortingGroup>();
            // Ensure the Inventory Slot is unselected by default.
            Unselect();
        }

        /// <summary>
        /// Initializes all required references and properties to ensure the InventorySlot works accordingly.
        /// </summary>
        /// <param name="row">Vertical Position of the Inventory Slot.</param>
        /// <param name="col">Horizontal Position of the Inventory Slot</param>
        /// <param name="data">Initial SlotData stored.</param>
        /// <param name="weaponController">Reference to the Player�s WeaponController</param>
        /// <param name="inventoryManager">Reference to the InventoryProManager</param>
        public void Initialize(int row, int col, SlotData data, WeaponController weaponController, InventoryProManager inventoryManager)
        {
            this.inventoryManager = inventoryManager;
            defaultBackgroundColor = backgroundImage.color;

            this.row = row;
            this.col = col;
            this.slotData = data;
            this.weaponController = weaponController;

            // Once the required information is filled in, update the slot graphics.
            UpdateSlotGraphics();

            // Ensure max amount on Weapons & attachments is 1.
            if (slotData.item is Weapon_SO || slotData.item is AttachmentIdentifier_SO) slotData.amount = 1;
        }
        #endregion

        #region input-detection

        // Calls when a drag happens
        public void OnBeginDrag(PointerEventData eventData)
        {
            InventorySlot selectedAnchorSlot = GetAnchorSlot();
            inventoryManager.OnBeginDrag(selectedAnchorSlot, AllowBeginDrag(selectedAnchorSlot));
        }

        // Calls while the drag happens
        public void OnDrag(PointerEventData eventData) { }

        // Calls when the slot is hovered
        public void OnPointerEnter(PointerEventData eventData)
        {
            Invoke(nameof(ShowTooltip), timeToShowTooltip);
            sortingGroup.sortingOrder = 100; // Avoids clipping with other slots
            inventoryManager.OnPointerEnter(this);
        }

        // Calls when the slot is not hovered anymore
        public void OnPointerExit(PointerEventData eventData)
        {
            CancelInvoke(nameof(ShowTooltip));
            sortingGroup.sortingOrder = 15;
            inventoryManager.OnPointerExit(defaultBackgroundColor);
        }
        // Calls when a Drag action finished
        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryManager.OnEndDrag(true);
        }
        // Calls when The Inventory Slot is clicked.
        public void OnPointerClick(PointerEventData eventData)
        {
            // If the Input was a right click, avoid running the click logic.
            if (eventData.button != PointerEventData.InputButton.Right) return;

            inventoryManager.onSlotRightClick?.Invoke(GetAnchorSlot());
        }

        #endregion

        #region utilities

        /// <summary>
        /// Displays a tooltip with the name and description of the stored Item in SlotData
        /// </summary>
        private void ShowTooltip()
        {
            // First ensure we can show the tooltip ( InventorySlot has a valid Item )
            if (CanShowTooltip())
            {
                //Always show the tooltip based on the Anchor�s Item
                InventorySlot anchor = GetAnchorSlot();
                inventoryManager._TooltipManager.SetTooltipVisibility(true, anchor.ItemName, anchor.ItemDescription);
            }
        }

        /// <summary>
        /// Returns whether the tooltip can be shown or not.
        /// </summary>
        private bool CanShowTooltip()
        {
            InventorySlot anchor = GetAnchorSlot();
            return anchor.slotData != null && anchor.slotData.item != null && anchor.slotData.amount > 0;
        }

        /// <summary>
        /// Updates a specific InventorySlot visuals. Displays the Icon of the stored Item in SlotData, and displays the amount ( If Item is not null ).
        /// In case Item is null, it will reset the graphics to the default/blank state.
        /// </summary>
        public void UpdateSlotGraphics()
        {
            // First check if this slot is anchored 
            if (anchorSlot && !IsHotbarSlot)
            {
                // Initially set the amount text to false, it will be enabled later if required.
                amountText.gameObject.SetActive(false);

                // This is the anchor slot = origin of the slot
                if (anchorSlot == this)
                {
                    // Update Icon�s sprite & sizing.
                    // If Inventory Style is Tetris, GatherSprite can return any size ( ex: 1x1, 2x1, 2x2, etc... )
                    // If it�s Grid, it can only return 1x1. 
                    iconImage.gameObject.SetActive(true);
                    iconImage.sprite = ItemIcon;
                    Vector2Int originalSize = GetItemSize();

                    // Original size is 1x1
                    if (originalSize == new Vector2Int(1, 1))
                    {
                        // Set the Amount Text but only display it if amount is greater than 1
                        amountText.text = slotData.amount.ToString();
                        amountText.gameObject.SetActive(slotData?.amount > 1 && slotData?.item ? true : false);
                    }
                    Vector2 newSize = transform.localScale * (Vector2)originalSize;

                    if (iconRect == null)
                        iconRect = iconImage.GetComponent<RectTransform>();

                    // Handle Icon orientation & sizing
                    bool isOriented = slotData.isOriented;
                    iconRect.localScale = isOriented ? newSize : new Vector2(newSize.y, newSize.x);
                    iconRect.localRotation = isOriented ? Quaternion.identity : Quaternion.Euler(0, 0, -90);
                    iconRect.pivot = new Vector2(0, isOriented ? 1 : 0.5f);
                    iconRect.anchorMin = iconRect.anchorMax = new Vector2(isOriented ? 0 : 0.5f, 1);

                    float adjustedX = iconImageMargins.x + (iconRect.rect.width / 2 * originalSize.x) + (10 * (originalSize.x - 1));
                    iconRect.localPosition = isOriented ? iconImageMargins : new Vector3(adjustedX, iconImageMargins.y, 0);

                    // Prevent the icons from being behind other buttons
                    this.transform.SetAsLastSibling();
                }
                else
                {
                    // This InventorySlot is anchored to a slot that�s not itself. This can only happen in Tetris Inventory.
                    // Since the Icon is handled by the slot, deactivate Icon Image here.
                    iconImage.sprite = null;
                    iconImage.gameObject.SetActive(false);
                    // Amount Text needs to be displayed at the right bottom corner always.
                    if (row == anchorSlot.row + anchorSlot?.GetItemSize().x - 1 &&
                        col == anchorSlot.col + anchorSlot?.GetItemSize().y - 1)
                    {
                        amountText.text = anchorSlot.slotData.amount.ToString();
                        amountText.gameObject.SetActive(anchorSlot.slotData?.amount > 1 && anchorSlot.slotData?.item ? true : false);
                    }
                }
                return;

            }

            // Restart Visuals to Default/Blank if no SlotData or Item is stored
            if ((slotData == null || slotData.amount <= 0 || slotData.item == null) && !anchorSlot)
            {
                iconImage.sprite = null;
                iconImage.gameObject.SetActive(false);
                amountText.gameObject.SetActive(false);
                return;
            }

            iconImage.gameObject.SetActive(true);
            iconImage.sprite = ItemIcon;
            amountText.text = slotData.amount.ToString();
            amountText.gameObject.SetActive(slotData?.amount > 1 && slotData?.item ? true : false);
        }

        /// <summary>
        /// Resets the Slot Data of an Ivnentory Slot & Updates its Visuals
        /// </summary>
        public void ClearSlot()
        {
            slotData = new SlotData();
            anchorSlot = null;
            UpdateSlotGraphics();
        }

        /// <summary>
        /// Returns current Anchor Slot. If Anchor Slot is null, it will return itself, as it cannot be anchored to anything else.
        /// </summary>
        public InventorySlot GetAnchorSlot()
        {
            return anchorSlot != null ? anchorSlot : this;

        }

        // Returns true if a Drag action can begin
        protected bool AllowBeginDrag(InventorySlot anchor)
        {
            return anchor.slotData != null && anchor.slotData.item != null && anchor.slotData.amount > 0;
        }

        /// <summary>
        /// Returns the Item Size of the Item in SlotData.
        /// If Item is null, a default 1x1 size will be returned.
        /// </summary>
        public Vector2Int GetItemSize()
        {
            if (slotData.item == null) return new Vector2Int(1, 1);

            Vector2Int calculatedItemSize = inventoryManager.IsGridInventory() ? new Vector2Int(1, 1) : slotData.item.itemSize;
            calculatedItemSize = slotData.isOriented ? calculatedItemSize : new Vector2Int(calculatedItemSize.y, calculatedItemSize.x);
            return calculatedItemSize;
        }

        /// <summary>
        /// Highlights this Inventory Slot and represents it visually. Often used by Gamepad / Controller.
        /// </summary>
        public void Select()
        {
            if (!isActiveAndEnabled) return;
            highlightedBorder.SetActive(true);
            StartCoroutine(SmoothScaleCoroutine(Vector3.one * highlightedScale));
        }

        /// <summary>
        /// Unselects this slot and represents it visually. Often used by Gamepad / Controller.
        /// </summary>
        public void Unselect()
        {
            highlightedBorder.SetActive(false);
            CancelInvoke(nameof(ShowTooltip));
            if (gameObject.activeInHierarchy) StartCoroutine(SmoothScaleCoroutine(normalScale));
            else inventoryContainer.localScale = normalScale;
        }

        // Handles scaling when hovered/unhovered
        private IEnumerator SmoothScaleCoroutine(Vector3 targetScale)
        {
            float elapsedTime = 0f;
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * lerpSpeed;
                inventoryContainer.localScale = Vector3.Lerp(inventoryContainer.localScale, targetScale, Time.deltaTime * lerpSpeed);
                yield return null;
            }

            // Ensure the final scale is exactly the target scale
            inventoryContainer.localScale = targetScale;
        }

        public bool IsSlotsInSameInventory(InventorySlot other)
        {
            return (this.IsChestSlot && other.IsChestSlot) || (this.IsHotbarSlot && other.IsHotbarSlot) || (this.IsInventorySlot && other.IsInventorySlot);
        }
        #endregion
    }

}