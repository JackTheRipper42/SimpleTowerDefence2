using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class OptionsPanel : MonoBehaviour
    {
        public Dropdown QualityDropdown;
        public Dropdown ResolutionDropdown;
        public Toggle WindowedToggle;

        public void OkClicked()
        {
            QualitySettings.SetQualityLevel(QualityDropdown.value);
            var resolution = Screen.resolutions[ResolutionDropdown.value];
            Screen.SetResolution(resolution.width, resolution.height, !WindowedToggle.isOn, resolution.refreshRate);
        }

        public void CancelClicked()
        {
            QualityDropdown.value = QualitySettings.GetQualityLevel();
            ResolutionDropdown.value = Screen.resolutions.ToList().IndexOf(Screen.currentResolution);
            WindowedToggle.isOn = !Screen.fullScreen;
        }

        protected virtual void Start()
        {
            QualityDropdown.AddOptions(QualitySettings.names.ToList());
            QualityDropdown.value = QualitySettings.GetQualityLevel();
            ResolutionDropdown.AddOptions(Screen.resolutions.Select(resolution => resolution.ToString()).ToList());
            ResolutionDropdown.value = Screen.resolutions.ToList().IndexOf(Screen.currentResolution);
            WindowedToggle.isOn = !Screen.fullScreen;
        }
    }
}
