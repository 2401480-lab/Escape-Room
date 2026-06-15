using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeGame
{
    public class SceneLoader : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}