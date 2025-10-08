#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneSwitcher : MonoBehaviour
{
    [MenuItem("Switch to Scene/Main")]
    public static void SwitchToScene1()
    {
        EditorSceneManager.OpenScene("Assets/Game/Scenes/MainGame.unity", OpenSceneMode.Single);
    }

    [MenuItem("Switch to Scene/Hub")]
    public static void SwitchToScene2()
    {
        EditorSceneManager.OpenScene("Assets/Game/Scenes/Hub.unity");
    }
}
#endif