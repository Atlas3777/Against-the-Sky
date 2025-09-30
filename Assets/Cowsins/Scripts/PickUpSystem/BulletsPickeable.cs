using UnityEngine;
#if INVENTORY_PRO_ADD_ON
using cowsins.Inventory;
#endif
namespace cowsins
{
    public partial class BulletsPickeable : Pickeable
    {
        [Tooltip("How many bullets you will get"), SerializeField, SaveField] private int amountOfBullets;

        [SerializeField] private BulletsItem_SO bulletsSO;

        [SerializeField] private Sprite bulletsIcon;

        [SerializeField] private GameObject bulletsGraphics;

        public int AmountOfBullets => amountOfBullets;

        public override void Awake()
        {
            base.Awake();
            image.sprite = bulletsIcon;
            Destroy(graphics.transform.GetChild(0).gameObject);
            Instantiate(bulletsGraphics, graphics);
        }
        public override void Interact(Transform player)
        {
            if (bulletsSO == null)
            {
                Debug.LogError("<color=red>[COWSINS]</color> <b><color=yellow>Bullet_SO</color></b> " +
                "not found! Skipping Interaction.", this);
                return;
            }
#if INVENTORY_PRO_ADD_ON
            if (InventoryProManager.instance)
            {
                (bool success, int remainingAmount) = InventoryProManager.instance._GridGenerator.AddItemToInventory(bulletsSO, amountOfBullets);
                if (success)
                {
                    interacted = true;
                    interactableEvents.OnInteract?.Invoke();
                    StoreData();
                    ToastManager.Instance?.ShowToast($"x{amountOfBullets - remainingAmount} {ToastManager.Instance.CollectedMsg}");
                    amountOfBullets = remainingAmount;
                    if(amountOfBullets <= 0) Destroy(this.gameObject);
                }
                else
                    ToastManager.Instance?.ShowToast(ToastManager.Instance.InventoryIsFullMsg);
                return;
            }
#else
            if (player.GetComponent<WeaponController>().weapon == null) return;
#endif
            interacted = true; 
            base.Interact(player);
            player.GetComponent<WeaponController>().id.totalBullets += amountOfBullets;
            Destroy(this.gameObject);
        }
        public void SetBullets(BulletsItem_SO bulletsSO, int amountOfBullets)
        {
            this.amountOfBullets = amountOfBullets;
            this.bulletsSO = bulletsSO;
        }

        public override bool IsForbiddenInteraction(WeaponController weaponController)
        {
            return AddonManager.instance.isInventoryAddonAvailable
                ? false
                : weaponController.weapon != null && !weaponController.weapon.limitedMagazines || weaponController.weapon == null;
        }

#if SAVE_LOAD_ADD_ON
        // Destroy if picked up.
        // Interacted State is called after loading.
        public override void LoadedState()
        {
            if (this.interacted) Destroy(this.gameObject);
        }
#endif
    }
}