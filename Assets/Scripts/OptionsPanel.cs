using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class OptionsPanel : MonoBehaviour
    {
        public Dropdown QualityDropdown;

        public void OkClicked()
        {
            QualitySettings.SetQualityLevel(QualityDropdown.value);
        }

        public void CancelClicked()
        {
            QualityDropdown.value = QualitySettings.GetQualityLevel();
        }

        protected virtual void Start()
        {
            QualityDropdown.AddOptions(QualitySettings.names.ToList());
            QualityDropdown.value = QualitySettings.GetQualityLevel();
        }
    }
}
