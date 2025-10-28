using UnityEngine;

[DefaultExecutionOrder(-9999)]
public class ServicedMain : MonoBehaviour
{
    static bool isInitialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Instantiate()
    {
        if (!isInitialized)
        {
            GameObject main = new GameObject("ServicedMain");
            main.AddComponent<ServicedMain>();
            DontDestroyOnLoad(main);
            isInitialized = true;
        }
    }

    private void Awake()
    {
        Debug.Log("======");
        Debug.Log("entrypoint hit");
    }
}