using System;
using UnityEngine;
using System.Collections.Generic;

namespace cowsins.Inventory
{
    public class CraftingUI : TradeUI
    {
        [SerializeField, Title("CRAFTING UI", upMargin = 10), Tooltip("Button that contains a Recipe to craft")] private CraftButton craftButton;
        [SerializeField, Tooltip("IngredientUIIcon that defines an ingredient for the Recipe of the highlighted CraftButton")] private IngredientUIIcon ingredientPrefab;
        [SerializeField, Tooltip("Ingredients Prefabs will be instantiated inside ingredientContainer")] private Transform ingredientContainer;

        // INTERNAL USE
        // For performance reasons, Ingredients UIs are stored in a pool so the Icons can be easily reused.
        private List<IngredientUIIcon> ingredientPool = new List<IngredientUIIcon>();
        private CraftingBench craftingBench;
        private List<TradeButton> buttonList = new List<TradeButton>();

        // Ensure no item is initially highlighted.
        private void OnEnable() => UpdateSelectedItemUI(null, 1);

        public void InitializeUI(Recipe_SO[] recipes, CraftingBench craftingBench, Action closeCraftingBench)
        {
            this.craftingBench = craftingBench;

            buttonList.Clear();

            // Base TradeUI method to individually initialize buttons.
            InitializeButtons(recipes, // Pass All Recipes
                item => item?.result != null, // Method to define if the item is valid
                (item, btn) => {
                    ((CraftButton)btn).Initialize(this, 
                    item, 
                    craftingBench, 
                    InventoryProManager.instance._GridGenerator.HasEnoughIngredients(item, 1),
                    GetCraftingProcess(item));
                    buttonList.Add(btn);
                }, // Method that initializes the CraftButton
                craftButton); // Trade Button to initialize

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => closeCraftingBench?.Invoke());
        }

        /// <summary>
        /// Highlights the current recipe and displays information about it: Name, result, ingredients, etc...
        /// </summary>
        /// <param name="recipe"></param>
        /// <param name="amountToCraft"></param>
        public void UpdateSelectedItemUI(Recipe_SO recipe, int amountToCraft)
        {
            // Ensure the passed recipe is not null
            if (recipe == null)
            {
                selectedItemInformation.SetActive(false);
                return;
            }

            // Set Basic Information
            selectedItemInformation.SetActive(true);
            iconImage.sprite = recipe.result.icon;
            nameText.text = recipe.result._name;

            // Update Ingredients
            UpdateIngredientDisplay(recipe, amountToCraft);
        }

        /// <summary>
        /// Updates the Ingredient Displays of a highlighted Recipe based on the desired amount of items to craft.
        /// </summary>
        /// <param name="recipe"></param>
        /// <param name="amountToCraft"></param>
        public void UpdateIngredientDisplay(Recipe_SO recipe, int amountToCraft)
        {
            int ingredientCount = recipe.ingredients.Length;

            // Expand pool if needed
            while (ingredientPool.Count < ingredientCount)
            {
                IngredientUIIcon newIngredient = Instantiate(ingredientPrefab, ingredientContainer);
                ingredientPool.Add(newIngredient);
            }

            // Update existing ingredient UIs
            for (int i = 0; i < ingredientPool.Count; i++)
            {
                if (i < ingredientCount)
                {
                    ingredientPool[i].gameObject.SetActive(true);
                    Item_SO item = recipe.ingredients[i].item;
                    int quantity = recipe.ingredients[i].quantity; 
                    bool hasEnough = InventoryProManager.instance._GridGenerator.HasEnoughOfItem(item, quantity * amountToCraft);
                    ingredientPool[i].SetIngredient(item, quantity * amountToCraft, hasEnough);
                }
                else
                {
                    ingredientPool[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Updates a Recipe Button progress.
        /// </summary>
        /// <param name="craftingProcess"></param>
        /// <param name="progress"></param>
        public void UpdateButtonProgress(CraftingProcess craftingProcess, float progress)
        {
            CraftButton craftButton = GetCraftButton(craftingProcess.recipe);
            if (craftButton == null) return; 
            craftButton.UpdateProgressIndicatorFill(progress);
            if (craftingProcess.remainingAmountToCraft <= 0) craftButton.FinishCrafting();
            craftButton.UpdateCollectText(craftingProcess.totalCrafted);
            craftButton.SetCurrentAmountText(); 
        }
        public void UpdateCraftingProgress(CraftingProcess craftingProcess, float progress)
        {
            CraftButton craftButton = GetCraftButton(craftingProcess.recipe);
            if (craftButton != null)
            {
                craftButton.UpdateProgressIndicatorFill(progress);
                if(craftingProcess.remainingAmountToCraft <= 0) craftButton.FinishCrafting();
            }
        }

        /// <summary>
        /// Returns the Craft Button based on its recipe.
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public CraftButton GetCraftButton(Recipe_SO recipe)
        {
            return buttonList.Find(btn => btn is CraftButton craftBtn && craftBtn.RecipeSO == recipe) as CraftButton;
        }

        /// <summary>
        /// Returns the crafting process of a specific recipe.
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public CraftingProcess GetCraftingProcess(Recipe_SO recipe)
        {
            return craftingBench.CraftingProcesses.Find(btn => btn is CraftingProcess craftBtn && craftBtn.recipe == recipe) as CraftingProcess;
        }

        /// <summary>
        /// Starts the Crafting Process for a Recipe based on the amount to craft.
        /// </summary>
        /// <param name="recipe"></param>
        /// <param name="craftAmount"></param>
        /// <returns></returns>
        public bool CraftItem(Recipe_SO recipe, int craftAmount)
        {
            if (!InventoryProManager.instance._GridGenerator.HasEnoughIngredients(recipe, craftAmount))
                return false;

            craftingBench.StartCraftingProcess(recipe, craftAmount);

            return InventoryProManager.instance._GridGenerator.PayIngredients(recipe, craftAmount); ;
        }

        public bool IsCrafting(Recipe_SO recipe)
        {
            return GetCraftingProcess(recipe).isCrafting; 
        }

        public void UpdateCraftingProcess(Recipe_SO recipe, int totalCrafted)
        {
            GetCraftingProcess(recipe).totalCrafted = totalCrafted;
#if SAVE_LOAD_ADD_ON
            craftingBench.StoreData();
#endif
        }

        public void UpdateCollectButton(Recipe_SO recipe, int craftedAmount)
        {
            CraftButton craftButton = GetCraftButton(recipe);
            if (craftButton != null)
            {
                craftButton.UpdateCollectText(craftedAmount);
            }
        }

        public void RefreshRecipeButtons()
        {
            foreach (Transform child in buttonsParent)
            {
                CraftButton button = child.GetComponent<CraftButton>();
                button.Initialize(this, button.RecipeSO, craftingBench, InventoryProManager.instance._GridGenerator.HasEnoughIngredients(button.RecipeSO, 1),
                    GetCraftingProcess(button.RecipeSO));
            }
        }
    }
}
