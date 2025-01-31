using System;
using OpenSkiJumping.Competition;
using OpenSkiJumping.Competition.Runtime;
using OpenSkiJumping.Scripts2025;
using OpenSkiJumping.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace OpenSkiJumping.UI
{
    public class WindGatePanel : MonoBehaviour
    {
        [SerializeField] private JumpSimulator simulator;
        [SerializeField] private GameplayExtension gameplayExtension;
        [SerializeField] private CompetitionRunner competitionRunner;

        [SerializeField] private TMP_Text gateText;
        [SerializeField] private Slider gateSlider;
        [SerializeField] private Button autoGateButton;

        [SerializeField] private TMP_Text windText;
        [SerializeField] public Slider windSlider;
        [SerializeField] private bool jumpOverHSPerformed = false;
        public void Awake()
        {
            gateSlider.onValueChanged.AddListener(UpdateGateText);
            gateSlider.onValueChanged.AddListener(UpdateCPUWinDistanceAfterGateChange);
            gateSlider.onValueChanged.AddListener(StoreGate);
            windSlider.onValueChanged.AddListener(UpdateWindText);
            windSlider.onValueChanged.AddListener(UpdateCPUWinDistanceAfterWindChange);
            autoGateButton.onClick.AddListener(SetAutoGate);
        }

        private void UpdateGateText(float val) => gateText.text = $"Gate: {val}";

        private void UpdateWindText(float val)
        {
            windText.text = $"Wind: {val:F2} m/s";
            autoGateButton.interactable = true;
        }

        private void UpdateCPUWinDistanceAfterGateChange(float val)
        {
            float modifier = 1;
            //simulator.CPUWinnerDistanceAfterGateChange((int)val, windSlider.value);

            //Add modifier for positive and negative wind
            
            /*if (windSlider.value > 0)
            {
                modifier *= 0.1f;
            }

            if (windSlider.value < 0)
            {
                modifier *= 2f;
            }*/

            simulator.CPUWinnerDistanceAfterGateChange((int)val, windSlider.value * (competitionRunner.GetHS() / 100f) * modifier);
        }

        private void UpdateCPUWinDistanceAfterWindChange(float val)
        {
            float modifier = 1;
            //Add modifier for positive and negative wind
            if (windSlider.value > 0)
            {
                modifier *= 0.9f;
            }

            if (windSlider.value < 0)
            {
                modifier *= 3f;
            }
            //Constant added to maintain balance between CPU and 1P
            simulator.GetGateForWind(windSlider.value * (competitionRunner.GetHS() / 100f) * modifier);
        }

        public void SetAutoGate()
        {
            gateSlider.value = simulator.GetGateForWind(windSlider.value);
            Debug.Log($"Calculated gate: {windSlider.value}");
            autoGateButton.interactable = false;
        }

        public void StoreGate(float val)
        {
            gameplayExtension.storeGate = gateSlider.value;
        }

        public void LowerGateAfterLongJump()
        {
  

                // Chyba czeba bydzie zrobiæ property z d³ugoœci¹ skoku CPU w Runtime Results Managerze. Albo storowaæ go w SO Gameplay extension!
                if (jumpOverHSPerformed == true || gameplayExtension.storeCPUDistance >= (decimal)competitionRunner.GetHS())
                {
                Debug.Log("LowerGateAfterLongJump conditions met, before Random Range");
                if (Random.Range(0, 100) < (int)gameplayExtension.gateDownChance)
                {
                    UnityEngine.Debug.Log("LowerGateAfterLongJump performed. gameplayExtension.gateDownChance: " + gameplayExtension.gateDownChance);
                    if (gateSlider.value != 1)
                    {
                        gateSlider.value -= 1;
                    }
                }
            }

            jumpOverHSPerformed = false;
        }

        public void JumpOverHSPerformed()
        {
            
            jumpOverHSPerformed = true;
        }


        public void StoreGate()
        {
            gameplayExtension.storeGate = gateSlider.value;
        }

        public void SetRandomWind()
        {
            windSlider.value += Random.Range((gameplayExtension.modifierWindRandomnessLevel / -200f), (gameplayExtension.modifierWindRandomnessLevel/200f));
            if(windSlider.value > 5f)
            {
                windSlider.value = 5f;
            }
            if (windSlider.value < -5f)
            {
                windSlider.value = -5f;
            }
            UpdateWindText(windSlider.value);
        }


        public void Initialize(int gates)
        {
            gateSlider.minValue = 1;
            gateSlider.maxValue = gates;
            windSlider.SetValueWithoutNotify(0);
            UpdateWindText(0);
            SetAutoGate();
        }
    }
}