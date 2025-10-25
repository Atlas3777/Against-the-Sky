using System;
using UnityEngine;
using UnityEngine.UI;

namespace cowsins.Inventory
{
    public class ShopUI : TradeUI
    {
        [SerializeField, Tooltip("Buttons to instantiate in the Shop.")] private ShopButton shopButton;
        [SerializeField, Tooltip("When purchasing Weapons, slidersContainer is enabled to display Weapons Statistics.")] private GameObject slidersContainer;
        [SerializeField, Tooltip("Contained in slidersContainer, when purchasing weapons each slider gets filled to represent their respective statistics.")] 
        private Slider damageSlider, fireRateSlider, magazineSizeSlider, speedModifierSlider;
        private Shop shop;
        public void InitializeUI(ShopItemData[] shopItems, TradeHub shop, Action closeShop)
        {
            InitializeButtons(shopItems,
                item => item.item != null,
                (item, btn) => ((ShopButton)btn).Initialize(this, item, shop, item.amountAvailable > 0),
                shopButton
            );
            this.shop = (Shop)shop;

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => closeShop?.Invoke());
        }

        /// <summary>
        /// Highlights the Item and displays its information in the shop.
        /// </summary>
        /// <param name="item"></param>
        public void UpdateHighlightedItemUI(Item_SO item)
        {
            if(item == null)
            {
                slidersContainer.SetActive(false);
                iconImage.gameObject.SetActive(false);
                nameText.gameObject.SetActive(false);
                return;
            }
           
            iconImage.gameObject.SetActive(true);
            nameText.gameObject.SetActive(true);

            // Only show the SLiders Container if the item to purchase is a Weapon, then fill the sliders accordingly.
            if (item is Weapon_SO weapon)
            {
                slidersContainer.SetActive(true);
                damageSlider.value = weapon.shootStyle == ShootStyle.Melee ? weapon.damagePerHit : weapon.damagePerBullet;
                fireRateSlider.value = weapon.fireRate;
                magazineSizeSlider.value = weapon.shootStyle == ShootStyle.Melee ? 1 : weapon.magazineSize;
                // speedModifierSlider.value = weapon.applyWeight ? 1 / weapon.weightMultiplier : 1; #MYTODO что-то про скорость от веса, пока отключу, там посмотрим
            }
            else
            {
                slidersContainer.SetActive(false);
            }

            iconImage.sprite = item?.icon;
            nameText.text = item._name;
        }

        /// <summary>
        /// Re-initializes all shop items. This is generally called after a purchase has occured so the shop displays if the Player has enough resources or not after the purchase
        /// </summary>
        /// <param name="purchasedShopButton"></param>
        public void RefreshShopItems(ShopButton purchasedShopButton)
        {
            coinsText.text = CoinManager.Instance.coins.ToString();
            foreach (Transform child in buttonsParent)
            {
                ShopButton shopButton = child.GetComponent<ShopButton>();
                if (shopButton == purchasedShopButton && shop.decreaseAvailableAmountOnPurchase)
                {
                    shopButton.ShopItemData.amountAvailable -= 1;
                }
                shopButton.Initialize(this, shopButton.ShopItemData, purchasedShopButton._TradeHub, shopButton.ShopItemData.amountAvailable > 0);
            }
        }
    }
}
