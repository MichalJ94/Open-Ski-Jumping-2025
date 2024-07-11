using System;
using OpenSkiJumping.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OpenSkiJumping.UI
{
    public class WindGatePanel : MonoBehaviour
    {
        [SerializeField] private JumpSimulator simulator;

        [SerializeField] private TMP_Text gateText;
        [SerializeField] private Slider gateSlider;
        [SerializeField] private Button autoGateButton;

        [SerializeField] private TMP_Text windText;
        [SerializeField] private Slider windSlider;

        public void Awake()
        {
            gateSlider.onValueChanged.AddListener(UpdateGateText);
            gateSlider.onValueChanged.AddListener(UpdateCPUWinDistanceAfterGateChange);
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