// using UnityEditor;
// using UnityEditor.SceneManagement;
// using UnityEngine.SceneManagement;
//
// namespace Game.Script.Editor
// {
//     [InitializeOnLoad]
//     public static class AutoStartScene
//     {
//         private const string StartScenePath = "Assets/InventoryPro_ADD-ON/Scenes/MainMenu_Inventory.unity";
//         //private const string StartScenePath = "Assets/Game/Scenes/MainGameKamikazeCopy.unity";
//         private static string _previousScenePath;
//
//         static AutoStartScene()
//         {
//             EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
//         }
//
//         private static void OnPlayModeStateChanged(PlayModeStateChange state)
//         {
//             switch (state)
//             {
//                 case PlayModeStateChange.ExitingEditMode:
//                     _previousScenePath = SceneManager.GetActiveScene().path;
//                     if (_previousScenePath != StartScenePath)
//                         if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
//                             EditorSceneManager.OpenScene(StartScenePath);
//                     break;
//                 
//                 case PlayModeStateChange.EnteredEditMode:
//                     if (!string.IsNullOrEmpty(_previousScenePath) && _previousScenePath != StartScenePath)
//                     {
//                         EditorSceneManager.OpenScene(_previousScenePath);
//                         _previousScenePath = null;
//                     }
//                     break;
//             }
//         }
//     }
// }