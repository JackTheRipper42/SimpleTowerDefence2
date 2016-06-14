using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class MainMenu : MonoBehaviour
    {
        public GameObject MainMenuPanel;
        public GameObject OptionsMenuPanel;

        private const string GameScene = "Game";

        protected virtual void Start()
        {
            Show(Menu.Main);
        }

        public void MainMenuStartClicked()
        {
            Show(Menu.None);
            StartCoroutine(LoadGameScene());
        }

        public void MainMenuOptionsClicked()
        {
            Show(Menu.Options);
        }

        public void MainMenuExitClicked()
        {
            Application.Quit();
        }

        public void OptionsOkClicked()
        {
            Show(Menu.Main);
        }

        public void OptionsCancelClicked()
        {
            Show(Menu.Main);
        }

        private void Show(Menu menu)
        {
            switch (menu)
            {
                case Menu.None:
                    MainMenuPanel.SetActive(false);
                    OptionsMenuPanel.SetActive(false);
                    break;
                case Menu.Main:
                    MainMenuPanel.SetActive(true);
                    OptionsMenuPanel.SetActive(false);
                    break;
                case Menu.Options:
                    MainMenuPanel.SetActive(false);
                    OptionsMenuPanel.SetActive(true);
                    break;
                default:
                    throw new NotSupportedException();
            }
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

        private enum Menu
        {
            None,
            Main,
            Options
        }
    }
}
