using UnityEngine;
using UnityEngine.SceneManagement;
namespace cowsins
{
    public class DeathRestart : MonoBehaviour
    {
        private void Update()
        {
            if (InputManager.reloading) SceneManager.LoadScene("Game/Scenes/Hub"); // #MYTODO пока чуть-чуть говнокод на перезапуске игры с хаба, потом можно будет поменять (хотя будто бы пофиг)
        }
    }
}