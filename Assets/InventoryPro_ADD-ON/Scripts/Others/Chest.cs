using UnityEngine;

namespace cowsins.Inventory
{
    public partial class Chest : Interactable
    {
        [SerializeField, Tooltip("Initial Content of this Chest"), Title("Chest")] private InventoryGridData_SO initialInventoryGridData;

        // Tracks the current content of the chest
        private SlotData[] currentChestData;
        // Size = rows * columns
        public Vector2Int ChestSize { get { return new Vector2Int(initialInventoryGridData.rows, initialInventoryGridData.columns); } }
        public InventoryGridData_SO InitialInventoryGridData => initialInventoryGridData;

        // Initially populate the content of the chest with initial inventory content.
        // This will be overrided by the Save & Load system if required.
        private void Start()
        {
            currentChestData = new SlotData[initialInventoryGridData.rows * initialInventoryGridData.columns];
            for (int i = 0; i < initialInventoryGridData.rows; i++)
            {
                for (int j = 0; j < initialInventoryGridData.columns; j++)
                {
                    int adjustedLinearIndex = i * initialInventoryGridData.columns + j;
                    currentChestData[adjustedLinearIndex] = initialInventoryGridData.inventoryInitialData[adjustedLinearIndex]?.Clone();
                    if (currentChestData[adjustedLinearIndex]?.item != null && currentChestData[adjustedLinearIndex].item is Weapon_SO)
                    {
                        Weapon_SO weapon = (Weapon_SO)currentChestData[adjustedLinearIndex].item;
                        currentChestData[adjustedLinearIndex].bulletsLeftInMagazine = weapon.magazineSize;
                        currentChestData[adjustedLinearIndex].totalBullets = weapon.limitedMagazines ? weapon.magazineSize * weapon.totalMagazines : weapon.magazineSize;
                        currentChestData[adjustedLinearIndex].barrel = weapon.weaponObject.barrel?.attachmentIdentifier;
                        currentChestData[adjustedLinearIndex].scope = weapon.weaponObject.scope?.attachmentIdentifier;
                        currentChestData[adjustedLinearIndex].stock = weapon.weaponObject.stock?.attachmentIdentifier;
                        currentChestData[adjustedLinearIndex].grip = weapon.weaponObject.grip?.attachmentIdentifier;
                        currentChestData[adjustedLinearIndex].magazine = weapon.weaponObject.magazine?.attachmentIdentifier;
                        currentChestData[adjustedLinearIndex].flashlight = weapon.weaponObject.flashlight?.attachmentIdentifier;
                        currentChestData[adjustedLinearIndex].laser = weapon.weaponObject.laser?.attachmentIdentifier;
                    }
                }
            }
        }

        public override void Interact(Transform player)
        {
            if (InventoryProManager.instance == null) return;

            // Opening the chest means generating a grid exclusively for chests apart from the Inventory´s Grid, given the parameters of this specific chest.
            InventoryProManager.instance._GridGenerator.OpenChest(this, currentChestData, initialInventoryGridData.rows, initialInventoryGridData.columns);
            // To visualize the Chest UI we need to Open the Inventory, as they work together
            InventoryProManager.instance.OpenInventory();

            interactableEvents.OnInteract?.Invoke();
            // Make sure we cannot move, shoot, or perform any action while the chest is opened.
            player.GetComponent<PlayerStats>().LoseControl();
        }

        public void SetChestData(SlotData[] slotData) => this.currentChestData = slotData;
    }
}