using System;
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

        [SerializeField] private TMP_Text gateText;
        [SerializeField] private Slider gateSlider;
        [SerializeField] private Button autoGateButton;

        [SerializeField] private TMP_Text windText;
        [SerializeField] private Slider windSlider;

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
            windText.text = $"Wind: {val} m/s";
            autoGateButton.interactable = true;
        }

        private void UpdateCPUWinDistanceAfterGateChange(float val)
        {
            simulator.CPUWinnerDistanceAfterGateChange((int)val, windSlider.value);
        }

        private void UpdateCPUWinDistanceAfterWindChange(float val)
        {
            simulator.GetGateForWind(windSlider.value);
        }

        private void SetAutoGate()
        {
            gateSlider.value = simulator.GetGateForWind(windSlider.value);
            Debug.Log($"Calculated gate: {windSlider.value}");
            autoGateButton.interactable = false;
        }

        public void StoreGate(float val)
        {
            gameplayExtension.storeGate = gateSlider.value;
        }

        public void StoreGate()
        {
            gameplayExtension.storeGate = gateSlider.value;
        }

        public void SetRandomWind()
        {
            windSlider.value += Random.Range(-0.5f, 0.5f);
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