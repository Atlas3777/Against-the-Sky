using UnityEngine;
using UnityEngine.SceneManagement;

namespace cowsins.SaveLoad
{
    public class SwitchSceneTrigger : Trigger
    {
        public string SceneToLoad;
        public override void TriggerEnter(Collider other)
        {
            if (DataPersistenceManager.instance == null)
            {
                ToastManager.Instance?.ShowToast(ToastManager.Instance?.DataPersistenceManagerNotAvailableMsg);
                Debug.Log("<color=red>[COWSINS]</color> Data Persistence Manager Not Found! To Save & Load your game, " +
                    "load the scene from the MainMenu or any other scene that includes DataPersistenceManager.");
                return;
            }
            SceneManager.LoadScene(SceneToLoad);
        }
    }
}
