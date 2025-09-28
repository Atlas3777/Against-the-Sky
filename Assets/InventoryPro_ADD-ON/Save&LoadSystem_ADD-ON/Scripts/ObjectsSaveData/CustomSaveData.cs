using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

namespace cowsins.SaveLoad
{
    [Serializable]
    public class CustomSaveData
    {
        // Stores the scene of the object to save
        // Useful to filter items that need to be loaded when playing the game,
        // to avoid trying to load objects that are not part of the current scene.
        [JsonProperty] public string SceneName { get; set; }

        // Position of the object to save. If no value is passed, it will be ignored and not serialized into the Json file.
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SerializableVector3 Position { get; set; }

        // Stores the type of the object to be saved.
        // Useful when it comes to defining Prefabs based on Types when loading Instantiated objects.
        // Check GameDataManager and TypeRegistry for more information.
        public string Type { get; set; }

        // Dictionary that contains all the saved variables and properties. 
        public Dictionary<string, object> savedFields = new Dictionary<string, object>();

        // Default Constructor
        public CustomSaveData()
        {

        }

        // Recommended Constructor
        public CustomSaveData(string sceneName, Vector3? position = null)
        {
            SceneName = sceneName;
            Position = position.HasValue ? new SerializableVector3(position.Value) : null;
            Type = this.GetType().Name;
        }

        // Stores all saveable obj´s fields into saveFields.
        // For a variable to be saved, add the [SaveField] and ensure the class inherits from Identifiable
        public object SaveFields(object obj)
        {
            if (obj == null) return null;

            var saveData = new CustomSaveData();
            // Gather all fields, whether they are public or not, we´ll filter them all based on the [SaveField] attribute
            var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            // Loop through each field
            foreach (var field in fields)
            {
                // Check if contains the [SaveField] attributes
                if (field.GetCustomAttribute<SaveFieldAttribute>() != null)
                {
                    object fieldValue = field.GetValue(obj);

                    // Skip null values
                    if (fieldValue == null) continue;

                    // Check for invalid numerical values to avoid unexpected errors
                    if (fieldValue is float f && (float.IsNaN(f) || float.IsInfinity(f)) ||
                        fieldValue is double d && (double.IsNaN(d) || double.IsInfinity(d)))
                    {
                        continue;
                    }

                    // If it's an Item_SO or a subclass, save only its name, since it is not a serializable value. We´ll then load it using the ItemRegistry
                    if (fieldValue is Item_SO item)
                    {
                        saveData.savedFields[field.Name] = item.name;
                    }
                    else
                    {
                        // Check for non-serializable types
                        if (!IsSerializable(fieldValue))
                        {
                            continue;
                        }
                        saveData.savedFields[field.Name] = fieldValue;
                    }
                }
            }

            // Ensure Scene & Type are filled. Posiiton is optional and won´t be included if null
            saveData.SceneName = SceneManager.GetActiveScene().name;
            saveData.Type = obj.GetType().Name;
            return saveData;
        }

        public void LoadFields(object obj, CustomSaveData saveData)
        {
            if (obj == null || saveData == null) return;

            // Gather all fields, whether they are public or not, we´ll filter them all based on the [SaveField] attribute
            var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var field in fields)
            {
                // Check if contains the [SaveField] attributes
                if (field.GetCustomAttribute<SaveFieldAttribute>() != null)
                {
                    if (saveData.savedFields.ContainsKey(field.Name))
                    {
                        object savedValue = saveData.savedFields[field.Name];

                        // Convert Ints
                        if (field.FieldType == typeof(int) && savedValue is long)
                        {
                            savedValue = Convert.ToInt32(savedValue);
                        }
                        // Convert Floats
                        else if (field.FieldType == typeof(float) && savedValue is double)
                        {
                            savedValue = Convert.ToSingle(savedValue);
                        }
                        // Handle Item_SO cases, since we cannot directly serialize-deserialize them
                        else if (typeof(Item_SO).IsAssignableFrom(field.FieldType))
                        {
                            if (savedValue != null && savedValue is string itemName)
                            {
                                Item_SO item = ItemRegistry.GetItemByName(itemName);
                                field.SetValue(obj, item);
                                continue;
                            }
                        }

                        // Set the field value after conversion
                        field.SetValue(obj, savedValue);
                    }
                }
            }

            // Handle scene name
            var sceneNameField = obj.GetType().GetField("SceneName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (sceneNameField != null && saveData.savedFields.ContainsKey("SceneName"))
            {
                sceneNameField.SetValue(obj, saveData.savedFields["SceneName"]);
            }
        }

        // Check if an object is serializable
        private bool IsSerializable(object obj)
        {
            Type type = obj.GetType();

            // Allow primitive types, enums, and common types
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
                return true;

            // Ensure the type is marked as serializable, otherwise, return false
            if (!type.IsSerializable)
                return false;

            if (obj is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item != null && !IsSerializable(item))
                        return false;
                }
            }

            return true;
        }
    }
}