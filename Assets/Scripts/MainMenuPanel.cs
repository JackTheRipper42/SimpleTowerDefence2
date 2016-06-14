using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class MainMenuPanel : MonoBehaviour
    {
        public Dropdown LevelDropdown;
        public Text DescriptionText;

        private Settings _settings;

        public void LevelChanged(int value)
        {
            _settings.LevelIndex = value;
            DescriptionText.text = _settings.Level.Description ?? string.Empty;
        }

        protected virtual void Start()
        {
            _settings = FindObjectOfType<Settings>();
            LevelDropdown.AddOptions(_settings.Levels.Select(level => level.Name).ToList());
            LevelDropdown.value = _settings.LevelIndex;
            DescriptionText.text = _settings.Level.Description ?? string.Empty;
        }
    }
}
