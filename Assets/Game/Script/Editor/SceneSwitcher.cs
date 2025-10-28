#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneSwitcher : MonoBehaviour
{
    [MenuItem("Switch to Scene/Winter")]
    public static void SwitchToScene1()
    {
        EditorSceneManager.OpenScene("Assets/Game/Scenes/Winter.unity");
    }
    [MenuItem("Switch to Scene/Hub")]
    public static void SwitchToScene2()
    {
        EditorSceneManager.OpenScene("Assets/Game/Scenes/Hub.unity");
    }

    [MenuItem("Switch to Scene/Menu")]
    public static void SwitchToScene3()
    {
        EditorSceneManager.OpenScene("Assets/InventoryPro_ADD-ON/Scenes/MainMenu_Inventory.unity");
    }
}
#endif