using System.Collections.Generic;
using UnityEngine.SceneManagement;
using cowsins.SaveLoad; 

namespace cowsins.Inventory
{
    public partial class Chest : Interactable
    {
#if SAVE_LOAD_ADD_ON
        // currentSlotData(SlotData[]) from Chest cannot be directly serialized, so we´ll store serializable data of each SlotData instead
        // Because of it, we need to create a new class that inherits from CustomSaveData. This class will store our customized data instead.
        [System.Serializable]
        public class ChestSaveData : CustomSaveData
        {
            public GameData.SerializableInventoryData[] serializedChestData;
        }

        // Base SaveFields method gathers all variables within the class that implement the [SaveField]
        // In this case, no variable implements it in Chest, so we´ll be processing and saving the slot datas manually.
        public override CustomSaveData SaveFields()
        {
            List<GameData.SerializableInventoryData> tempSerializedChestData = new List<GameData.SerializableInventoryData>(0);

            for (int i = 0; i < initialInventoryGridData.rows * initialInventoryGridData.columns; i++)
            {
                GameData.SerializableInventoryData tempSlotData = new GameData.SerializableInventoryData();

                // Populate all data into the Serializable Inventory Data class
                if (currentChestData.Length > i && currentChestData[i] != null)
                {
                    var data = currentChestData[i];
                    tempSlotData.SOPath = data.item?.name;
                    tempSlotData.isOriented = data.isOriented;
                    tempSlotData.amount = data.amount;
                    tempSlotData.bulletsLeftInMagazine = data.bulletsLeftInMagazine;
                    tempSlotData.totalBullets = data.totalBullets;
                    tempSlotData.barrel = data.barrel?.name;
                    tempSlotData.scope = data.scope?.name;
                    tempSlotData.stock = data.stock?.name;
                    tempSlotData.grip = data.grip?.name;
                    tempSlotData.magazine = data.magazine?.name;
                    tempSlotData.flashlight = data.flashlight?.name;
                    tempSlotData.laser = data.laser?.name;
                }
                else
                {
                    tempSlotData.SOPath = string.Empty;
                    tempSlotData.amount = 0;
                    tempSlotData.bulletsLeftInMagazine = 0;
                    tempSlotData.totalBullets = 0;
                }

                tempSerializedChestData.Add(tempSlotData);
            }

            return new ChestSaveData
            {
                serializedChestData = tempSerializedChestData.ToArray(),
                // Scene Name is also very important and must be saved.
                SceneName = SceneManager.GetActiveScene().name
            };
        }

        public override void LoadFields(object data)
        {
            if (data is ChestSaveData saveData)
            {
                SlotData[] tempData = new SlotData[saveData.serializedChestData.Length];
                currentChestData = new SlotData[tempData.Length];
                for (int i = 0; i < saveData.serializedChestData.Length; i++)
                {
                    Item_SO itemSO = ItemRegistry.GetItemByName(saveData.serializedChestData[i]?.SOPath) as Item_SO;
                    SlotData tempSlotData = new SlotData();
                    tempSlotData.item = itemSO;
                    tempSlotData.isOriented = saveData.serializedChestData[i].isOriented;
                    tempSlotData.amount = saveData.serializedChestData[i].amount;
                    tempSlotData.bulletsLeftInMagazine = saveData.serializedChestData[i].bulletsLeftInMagazine;
                    tempSlotData.totalBullets = saveData.serializedChestData[i].totalBullets;
                    tempSlotData.barrel = ItemRegistry.GetItemByName(saveData.serializedChestData[i]?.barrel) as AttachmentIdentifier_SO;
                    tempSlotData.scope = ItemRegistry.GetItemByName(saveData.serializedChestData[i]?.scope) as AttachmentIdentifier_SO;
                    tempSlotData.stock = ItemRegistry.GetItemByName(saveData.serializedChestData[i]?.stock) as AttachmentIdentifier_SO;
                    tempSlotData.grip = ItemRegistry.GetItemByName(saveData.serializedChestData[i]?.grip) as AttachmentIdentifier_SO;
                    tempSlotData.magazine = ItemRegistry.GetItemByName(saveData.serializedChestData[i]?.magazine) as AttachmentIdentifier_SO;
                    tempSlotData.flashlight = ItemRegistry.GetItemByName(saveData.serializedChestData[i]?.flashlight) as AttachmentIdentifier_SO;
                    tempSlotData.laser = ItemRegistry.GetItemByName(saveData.serializedChestData[i]?.laser) as AttachmentIdentifier_SO;
                    tempData[i] = tempSlotData;
                }
                currentChestData = tempData;
                base.LoadFields(data);
            }

        }
#endif
    }
}