using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class MainMenu : MonoBehaviour
    {
        private const string GameScene = "Game";

        protected virtual void Start()
        {
            StartCoroutine(LoadGameScene());
        }

        private static IEnumerator LoadGameScene()
        {
            yield return new WaitForEndOfFrame();

            var asyncOperation = SceneManager.LoadSceneAsync(GameScene, LoadSceneMode.Single);

            while (!asyncOperation.isDone)
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
