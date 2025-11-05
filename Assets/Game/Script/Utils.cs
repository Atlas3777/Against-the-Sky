using cowsins;
using cowsins.SaveLoad;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Utils
{
    public static void SwitchAndSaveScene(string SceneToLoad)
    {
        if (DataPersistenceManager.instance == null)
        {
            ToastManager.Instance?.ShowToast(ToastManager.Instance?.DataPersistenceManagerNotAvailableMsg);
            Debug.Log("<color=red>[COWSINS]</color> Data Persistence Manager Not Found! To Save & Load your game, " +
                      "load the scene from the MainMenu or any other scene that includes DataPersistenceManager.");
            return;
        }

        DataPersistenceManager.instance.SaveGame();
        Debug.Log("<color=green>[COWSINS]</color> Game successfully saved!");
        SceneManager.LoadScene(SceneToLoad);
    }

    public static bool IsTargetWithinAngle(Transform objTransform, Vector3 targetPosition, float angle)
    {
        var toTarget = (targetPosition - objTransform.position).normalized;
        var angleToTarget = Vector3.Angle(objTransform.forward, toTarget);
        //Debug.Log($"angleToTarget: {angleToTarget}, angle: {angle}, tvar: {angleToTarget <= (angle / 2f)}");
        return angleToTarget <= (angle / 2f);
    }

    public static bool HasClearView(Transform observer, Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - observer.position).normalized;
        float distance = Vector3.Distance(observer.position, targetPosition);
        if (Physics.Raycast(observer.position, direction, out RaycastHit hit, distance))
        {
            // Debug.Log("ray");
            if (!hit.collider.CompareTag("Player"))
            {
                //Debug.Log("not to player");
                return false;
            }
        }

        return true;
    }
}