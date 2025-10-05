#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace cowsins.SaveLoad
{
    public class ItemRegistryEditor : EditorWindow
    {
        public static void BuildItemRegistry()
        {
            // Create or load the ItemRegistry asset in the Resources folder. The Item Registry gets created if it is not found inside the Resources folder.
            ItemRegistry itemRegistry = Resources.Load<ItemRegistry>("ItemRegistry");

            if (itemRegistry == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                itemRegistry = CreateInstance<ItemRegistry>();
                AssetDatabase.CreateAsset(itemRegistry, "Assets/Resources/ItemRegistry.asset");
                AssetDatabase.SaveAssets();
            }

            // Collect all Item_SO assets in the project
            List<Item_SO> itemList = new List<Item_SO>();
            string[] guids = AssetDatabase.FindAssets("t:Item_SO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Item_SO item = AssetDatabase.LoadAssetAtPath<Item_SO>(path);
                if (item != null)
                {
                    itemList.Add(item);
                }
            }

            // Assign the collected items to the ItemRegistry
            itemRegistry.items = itemList;

            // Mark the ItemRegistry asset as dirty and save it
            EditorUtility.SetDirty(itemRegistry);
            AssetDatabase.SaveAssets();

            Debug.Log("ItemRegistry built with " + itemList.Count + " items.");
        }
    }
}
#endif
