using UnityEngine;
using System.Collections.Generic;

namespace cowsins.SaveLoad
{
    /// <summary>
    /// The Item Registry is a collection of all Item_SOs in your Project. Save & Load requires to access the stored references.
    /// </summary>
    [System.Serializable]
    public class ItemRegistry : ScriptableObject
    {
        private static ItemRegistry instance;
        public List<Item_SO> items = new List<Item_SO>();

        public static ItemRegistry Instance
        {
            get
            {
                if (instance == null) Initialize();
                return instance;
            }
        }

        static void Initialize()
        {
            instance = Resources.Load<ItemRegistry>("ItemRegistry");

            if (instance == null)
            {
                instance = CreateInstance<ItemRegistry>();
            }
        }

        // Runtime access to get an item by its name
        public static Item_SO GetItemByName(string itemName)
        {
            for (int i = Instance.items.Count - 1; i >= 0; i--)
            {
                Item_SO item = Instance.items[i];
                if (item == null)
                {
                    Instance.items.RemoveAt(i);
                }
                else if (item.name == itemName)
                {
                    return item;
                }
            }
            return null;
        }
    }
}