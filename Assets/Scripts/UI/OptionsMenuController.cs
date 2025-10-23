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
        [SerializeField] private Toggle useRandomEvents;
        [SerializeField] private Slider snowSlider;
        [SerializeField] private Slider windSlider;
        [SerializeField] private Slider gateDownSlider;
        [SerializeField] private Slider turbulenceSlider;
        [SerializeField] private Slider maxRandomEventsSlider;
        [SerializeField] private Slider maxRandomEventsSkillChangeSlider;

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

            useRandomEvents.SetIsOnWithoutNotify(gameConfig.Config.useRandomEvents);
            useRandomEvents.onValueChanged.AddListener(UpdateUseRandomEvents);


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


            if (gameConfig.Config.turbulenceChance != float.NaN)
            {

                turbulenceSlider.value = gameConfig.Config.turbulenceChance;
                gameplayExtension.turbulenceChance = turbulenceSlider.value;
            }
            else
            {
                turbulenceSlider.value = 10f;
                gameplayExtension.turbulenceChance = 10f;
            }
            turbulenceSlider.onValueChanged.AddListener(UpdateTurbulenceSlider);



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


            if (gameConfig.Config.maxRandomEvents != float.NaN)
            {

                maxRandomEventsSlider.value = gameConfig.Config.maxRandomEvents;
                gameplayExtension.maxRandomEvents = maxRandomEventsSlider.value;
            }
            else
            {
                maxRandomEventsSlider.value = 3f;
                gameplayExtension.maxRandomEvents = 3f;
            }
            maxRandomEventsSlider.onValueChanged.AddListener(UpdateMaxEventsSlider);

            if (gameConfig.Config.maxRandomEventsSkillChange != float.NaN)
            {

                maxRandomEventsSkillChangeSlider.value = gameConfig.Config.maxRandomEventsSkillChange;
                gameplayExtension.maxRandomEventsSkillChange = maxRandomEventsSkillChangeSlider.value;
            }
            else
            {
                maxRandomEventsSkillChangeSlider.value = 5f;
                gameplayExtension.maxRandomEventsSkillChange = 5f;
            }
            maxRandomEventsSkillChangeSlider.onValueChanged.AddListener(UpdateMaxSkillChangeSlider);


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


        private void UpdateUseRandomEvents(bool has)
        {
            gameConfig.Config.useRandomEvents = has;
            gameplayExtension.useRandomEvents = has;
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
            gameplayExtension.snowChance = snowSlider.value;
        }

        private void UpdateGateDownSlider(float arg)
        {
            gameConfig.Config.gateDownChance = gateDownSlider.value;
            gameplayExtension.gateDownChance = gateDownSlider.value;
        }

        private void UpdateTurbulenceSlider(float arg)
        {
            gameConfig.Config.turbulenceChance = turbulenceSlider.value;
            gameplayExtension.turbulenceChance = turbulenceSlider.value;
        }

        private void UpdateMaxEventsSlider(float arg)
        {
            gameConfig.Config.maxRandomEvents = maxRandomEventsSlider.value;
            gameplayExtension.maxRandomEvents = maxRandomEventsSlider.value;
        }

        private void UpdateMaxSkillChangeSlider(float arg)
        {
            gameConfig.Config.maxRandomEventsSkillChange = maxRandomEventsSkillChangeSlider.value;
            gameplayExtension.maxRandomEventsSkillChange = maxRandomEventsSkillChangeSlider.value;
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