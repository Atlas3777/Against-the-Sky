using UnityEngine;
using UnityEngine.SceneManagement;

namespace cowsins.SaveLoad
{
    public class SwitchAndSaveSceneTrigger : Trigger
    {
        public string SceneToLoad;

        public override void TriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Utils.SwitchAndSaveScene(SceneToLoad);
            }
        }
    }
}