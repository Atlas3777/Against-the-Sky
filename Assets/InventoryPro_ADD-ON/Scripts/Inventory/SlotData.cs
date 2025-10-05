namespace cowsins.Inventory
{
    /// <summary>
    /// Each Slot in the Inventory, Hotbar or Chest requires an initialized SlotData.
    /// SlotData contains essential information about the Item a slot carries, its amount, attachments, bullets, and much more.
    /// </summary>
    [System.Serializable]
    public class SlotData
    {
        public Item_SO item; // This can be Item_SO, Weapon_SO, BulletsItem_SO, AttachmentIdentifier_SO, etc...
        public int amount;
        public bool isOriented = true; // If the Inventory is Tetris style, you can rotate the items in the Inventory by pressing "R". If facing the default direction, isOriented = true;

        // Handle Attachment References for Weapons
        public AttachmentIdentifier_SO barrel,
              scope,
              stock,
              grip,
              magazine,
              flashlight,
              laser;
        // Handle Bullets for Weapons
        public int bulletsLeftInMagazine, totalBullets;

        // Constructors
        public SlotData(Item_SO item, int amount)
        {
            this.item = item;
            this.amount = amount;
        }

        // Default SlotData Constructor
        public SlotData()
        {
            item = null;
            amount = 0;
        }

        // Complete SlotData Constructor
        public SlotData(Item_SO item, int amount, int bulletsLeftInMagazine, int totalBullets,
                        AttachmentIdentifier_SO barrel, AttachmentIdentifier_SO scope,
                        AttachmentIdentifier_SO stock, AttachmentIdentifier_SO grip,
                        AttachmentIdentifier_SO magazine, AttachmentIdentifier_SO flashlight,
                        AttachmentIdentifier_SO laser)
        {
            this.item = item;
            this.amount = amount;
            this.bulletsLeftInMagazine = bulletsLeftInMagazine;
            this.totalBullets = totalBullets;
            this.barrel = barrel;
            this.scope = scope;
            this.stock = stock;
            this.grip = grip;
            this.magazine = magazine;
            this.flashlight = flashlight;
            this.laser = laser;
        }

        public SlotData Clone()
        {
            return new SlotData
            {
                item = this.item,
                amount = this.amount,   
                isOriented = this.isOriented,
                bulletsLeftInMagazine = this.bulletsLeftInMagazine,
                totalBullets = this.totalBullets,
                barrel = this.barrel,
                scope = this.scope,
                stock = this.stock,
                grip = this.grip,
                magazine = this.magazine,
                flashlight = this.flashlight,
                laser = this.laser
            };
        }
    }
}