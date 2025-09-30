using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace cowsins.SaveLoad
{
    // Helper class that handles Safe Json, Serialization, Encrypting & Decrypting Files.
    public class FileDataHandler
    {
        private string persistentDataDirPath = "";
        private string dataFileName = "";

        // File Data Handler Constructor
        public FileDataHandler(string dataFileName)
        {
            this.persistentDataDirPath = Application.persistentDataPath;
            this.dataFileName = dataFileName;
        }

        public void Save(GameData data, string profileId)
        {
            // We always need to save based on a Profile ID
            if (profileId == null) return;

            // The data is stored at the persistent data path, inside its corresponding profileID index, in a json file named after the specified dataFileName.
            string fullPath = Path.Combine(persistentDataDirPath, profileId, dataFileName);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                JsonSerializerSettings settings = GetJsonSerializerSettings();

                string dataToStore = JsonConvert.SerializeObject(data, settings);
                dataToStore = Encrypt(dataToStore);

                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(dataToStore);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error occurred when trying to save data to file: " + fullPath + "\n" + e);
            }
        }

        public GameData Load(string profileId)
        {
            // We always need to load based on a Profile ID
            if (profileId == null) return null;

            // The data is stored at the persistent data path, inside its corresponding profileID index, in a json file named after the specified dataFileName.
            string fullPath = Path.Combine(persistentDataDirPath, profileId, dataFileName);

            GameData loadedData = null;
            if (File.Exists(fullPath))
            {
                try
                {
                    string dataToLoad = "";
                    using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataToLoad = reader.ReadToEnd();
                        }
                    }

                    // Decrypt the Data & Deserialize it
                    dataToLoad = Decrypt(dataToLoad);

                    JsonSerializerSettings settings = GetJsonSerializerSettings();

                    loadedData = JsonConvert.DeserializeObject<GameData>(dataToLoad, settings);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error occurred when trying to load data from file: " + fullPath + "\n" + e);
                }
            }
            return loadedData;
        }


        // Deletes a Save Slot based on its profile ID
        public void Delete(string profileId)
        {
            if (profileId == null) return;

            // Gather the entire Path
            string fullPath = Path.Combine(persistentDataDirPath, profileId, dataFileName);
            try
            {
                // If the file exists, delete it
                if (File.Exists(fullPath))
                {
                    Directory.Delete(Path.GetDirectoryName(fullPath), true);
                }
                else
                {
                    Debug.LogWarning("Tried to delete profile data, but data was not found at path: " + fullPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to delete profile data for profileId; "
                    + profileId + " at Path: " + fullPath + "\n" + e);
            }
        }

        // Returns the most recently played Profile ID ( That corresponds to a Save Slot )
        public string GetMostRecentlyUpdatedProfileId()
        {
            string mostRecentProfileId = null;

            Dictionary<string, GameData> profileDictionary = LoadAllProfiles();

            foreach (KeyValuePair<string, GameData> pair in profileDictionary)
            {
                string profileId = pair.Key;
                GameData profileData = pair.Value;

                if (profileData == null)
                {
                    continue;
                }

                if (mostRecentProfileId == null)
                {
                    mostRecentProfileId = profileId;
                }
                else
                {
                    DateTime mostRecentDateTime = DateTime.FromBinary(profileDictionary[mostRecentProfileId].lastUpdated);
                    DateTime newDateTime = DateTime.FromBinary(profileData.lastUpdated);

                    if (newDateTime > mostRecentDateTime)
                    {
                        mostRecentProfileId = profileId;
                    }
                }
            }
            return mostRecentProfileId;
        }

        public Dictionary<string, GameData> LoadAllProfiles()
        {
            Dictionary<string, GameData> profileDictionary = new Dictionary<string, GameData>();

            IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(persistentDataDirPath).EnumerateDirectories();
            foreach (DirectoryInfo dirInfo in dirInfos)
            {
                string profileId = dirInfo.Name;

                string fullPath = Path.Combine(persistentDataDirPath, profileId, dataFileName);

                if (!File.Exists(fullPath))
                {
                    Debug.LogWarning("Skipping directory when loading all profiles because it does not contain data: " + profileId);
                    continue;
                }

                GameData profileData = Load(profileId);

                if (profileData != null)
                {
                    profileDictionary.Add(profileId, profileData);
                }
                else
                {
                    Debug.LogError("Tried to load profile but something went wrong. ProfileID: " + profileId);
                }
            }

            return profileDictionary;
        }

        // Default Json Serializer Settings
        private JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                TypeNameHandling = TypeNameHandling.Auto,  // Enable polymorphic deserialization ( Allows inheritance )
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,  // Avoid circular references
                SerializationBinder = new SafeSerializationBinder()
            };
        }

        // Returns an encrypted String based on the passed string
        private string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GetEncryptionKey();
                aes.IV = GetIVKey();

                using (MemoryStream memoryStream = new MemoryStream())
                using (CryptoStream cryptographyStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (StreamWriter writer = new StreamWriter(cryptographyStream))
                    {
                        writer.Write(plainText);
                    }
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        // Returns the passed string but decrypred
        private string Decrypt(string encryptedText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GetEncryptionKey();
                aes.IV = GetIVKey();

                using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(encryptedText)))
                using (CryptoStream cryptographyStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (StreamReader reader = new StreamReader(cryptographyStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private byte[] GetEncryptionKey()
        {        
            // XOR Encrypted Key
            string encryptionKey = "G2I+bSlsPG89bjJpMGgxazZqKmM1YjNtL2wjby5uKGk=";
            encryptionKey = XORDecrypt(encryptionKey, 0x5A);
            return Encoding.UTF8.GetBytes(encryptionKey);
        }
        private byte[] GetIVKey()
        {
            // XOR Encrypted IV Key
            string ivKey = "AmsAaBlpDG4YbxRsF20LYg==";
            ivKey = XORDecrypt(ivKey, 0x5A);
            return Encoding.UTF8.GetBytes(ivKey);
        }

        private string XORDecrypt(string input, byte key)
        {
            byte[] bytes = Convert.FromBase64String(input);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= key;
            }
            return Encoding.UTF8.GetString(bytes);
        }

#if UNITY_EDITOR
        // For security reasons, do not move EncryptDecryptXOREditorOnly out of #if UNITY_EDITOR conditional statement
        // And do not try to execute it anywhere else.
        // Storing this public method inside #if UNITY_EDITOR allows to ensure that this code won´t be put into production and will just be usable for Editor Purposes,
        // Ensuring cheaters cannot decrypt the data so easily.
        public string EncryptDecryptAESEditorOnly(string data)
        {
            return Decrypt(data);
        }
#endif

    }
}