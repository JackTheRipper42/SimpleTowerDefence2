using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class MainMenu : MonoBehaviour
    {
        public GameObject MainMenuPanel;
        public GameObject OptionsMenuPanel;
        public Dropdown LevelDropdown;
        public Text DescriptionText;
        public Dropdown QualityDropdown;

        private const string GameScene = "Game";

        private Settings _settings;

        protected virtual void Start()
        {
            _settings = FindObjectOfType<Settings>();
            LevelDropdown.AddOptions(_settings.Levels.Select(level => level.Name).ToList());
            LevelDropdown.value = _settings.LevelIndex;
            DescriptionText.text = _settings.Level.Description ?? string.Empty;

            QualityDropdown.AddOptions(QualitySettings.names.ToList());
            QualityDropdown.value = QualitySettings.GetQualityLevel();

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
            QualitySettings.SetQualityLevel(QualityDropdown.value);
            Show(Menu.Main);
        }

        public void OptionsCancelClicked()
        {
            QualityDropdown.value = QualitySettings.GetQualityLevel();
            Show(Menu.Main);
        }

        public void MainMenuLevelChanged(int value)
        {
            _settings.LevelIndex = value;
            DescriptionText.text = _settings.Level.Description ?? string.Empty;
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
