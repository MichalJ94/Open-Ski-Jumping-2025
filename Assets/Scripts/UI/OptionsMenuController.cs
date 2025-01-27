using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OpenSkiJumping.Data;
using OpenSkiJumping.Scripts2025;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace OpenSkiJumping.UI
{
    public class OptionsMenuController : MonoBehaviour
    {
        [SerializeField] private GameConfigRuntime gameConfig;

        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Dropdown languageDropdown;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private SegmentedControl fullScreenSelect;
        [SerializeField] private Slider randomnessSlider;
        [SerializeField] private GameplayExtension gameplayExtension;
        [SerializeField] private Slider snowSlider;
        [SerializeField] private Slider windSlider;
        [SerializeField] private Slider gateDownSlider;

        private List<Resolution> _resolutions;

        private void Start()
        {
            inputField.SetTextWithoutNotify(gameConfig.Config.mouseSensitivity.ToString(CultureInfo.InvariantCulture));
            inputField.onValueChanged.AddListener(UpdateSensitivity);

            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(gameConfig.Translations.Languages.Select(item => item.NativeLanguageName)
                .ToList());
            languageDropdown.SetValueWithoutNotify((int) gameConfig.Config.currentLanguage);
            languageDropdown.onValueChanged.AddListener(UpdateLanguage);

            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(QualitySettings.names.ToList());
            // Debug.Log(QualitySettings.names[QualitySettings.GetQualityLevel()]);
            
            qualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
            qualityDropdown.onValueChanged.AddListener(UpdateQuality);
            
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(Screen.resolutions.Select(it => $"{it.width} x {it.height}").ToList());
            _resolutions = Screen.resolutions.ToList();
            var resolutionIndex = _resolutions.IndexOf(Screen.currentResolution);
            resolutionDropdown.SetValueWithoutNotify(resolutionIndex);
            resolutionDropdown.onValueChanged.AddListener(UpdateResolution);

            fullScreenSelect.SetSelectedSegmentWithoutNotify(Screen.fullScreen == false ? 0 : 1);
            fullScreenSelect.onValueChanged.AddListener(UpdateFullScreen);

            if (gameConfig.Config.randomnessLevelCPU != 0)
            {
                
                randomnessSlider.value = gameConfig.Config.randomnessLevelCPU;
                gameplayExtension.modifierCPURandomnessLevel = randomnessSlider.value;
            }
            else
            {
                randomnessSlider.value = 20f;
                gameplayExtension.modifierCPURandomnessLevel = 20f;
            }
            randomnessSlider.onValueChanged.AddListener(UpdateRandomnessSlider);



            if (gameConfig.Config.randomnessLevelWind != 0)
            {

                windSlider.value = gameConfig.Config.randomnessLevelWind;
                gameplayExtension.modifierWindRandomnessLevel = windSlider.value;
            }
            else
            {
                randomnessSlider.value = 20f;
                gameplayExtension.modifierCPURandomnessLevel = 20f;
            }
            windSlider.onValueChanged.AddListener(UpdateWindSlider);


            if (gameConfig.Config.snowChance != float.NaN)
            {

                snowSlider.value = gameConfig.Config.snowChance;
                gameplayExtension.snowChance = snowSlider.value;
            }
            else
            {
                snowSlider.value = 10f;
                gameplayExtension.snowChance = 10f;
            }
            snowSlider.onValueChanged.AddListener(UpdateSnowSlider);

            if (gameConfig.Config.gateDownChance != float.NaN)
            {

                gateDownSlider.value = gameConfig.Config.gateDownChance;
                gameplayExtension.gateDownChance = gateDownSlider.value;
            }
            else
            {
                gateDownSlider.value = 80f;
                gameplayExtension.gateDownChance = 80f;
            }
            gateDownSlider.onValueChanged.AddListener(UpdateGateDownSlider);


        }

        private void UpdateQuality(int arg)
        {
            QualitySettings.SetQualityLevel(arg);
        }

        private void UpdateResolution(int arg)
        {
            Screen.SetResolution(_resolutions[arg].width, _resolutions[arg].height, Screen.fullScreen);
        }

        private void UpdateFullScreen(int arg)
        {
            Screen.fullScreen = arg != 0;
        }

        private void UpdateRandomnessSlider(float arg)
        {
            gameConfig.Config.randomnessLevelCPU = randomnessSlider.value;
            gameplayExtension.modifierCPURandomnessLevel = randomnessSlider.value;
        }

        private void UpdateWindSlider(float arg)
        {
            gameConfig.Config.randomnessLevelWind = windSlider.value;
            gameplayExtension.modifierWindRandomnessLevel = windSlider.value;
        }

        private void UpdateSnowSlider(float arg)
        {
            gameConfig.Config.snowChance = snowSlider.value;
        }

        private void UpdateGateDownSlider(float arg)
        {
            gameConfig.Config.gateDownChance = gateDownSlider.value;
        }

        private void UpdateSensitivity(string val)
        {
            gameConfig.Config.mouseSensitivity = float.Parse(val);
        }

        private void UpdateLanguage(int val)
        {
            gameConfig.SetLanguage((GameConfig.Language) val);
        }
    }
}