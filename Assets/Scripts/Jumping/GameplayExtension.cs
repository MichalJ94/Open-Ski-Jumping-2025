using OpenSkiJumping.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenSkiJumping.Scripts2025
{
    [CreateAssetMenu]
    public class GameplayExtension : ScriptableObject
    {
        // Start is called before the first frame update

        /*public float inrunDragModifier (int skill)
        {
            float modifier = 0.000005f;
            return (80-skill)*modifier;
             
        }*/
        [SerializeField] private GameConfigRuntime gameConfig;
        public float modifierCPURandomnessLevel;

        private void OnEnable()
        {
            if (gameConfig != null && gameConfig.Config != null)
            {
                modifierCPURandomnessLevel = gameConfig.Config.randomnessLevelCPU;
            }
            else
            {
                Debug.LogWarning("gameConfig or gameConfig.Config is null.");
            }
        }



        public float forceScaleModifier(int skill)
        {
            float modifier = 0.008f;
            if(skill > 80)
            {
                modifier = 0.009f;
            }
            return (80 - skill) * modifier;

        }


        public decimal CalculateFinalCPUDistance(decimal distance, int skill)
        {
            float modifier = 1;
            UnityEngine.Debug.Log("Od ResultsManager jumpData.JumperSKill: " + skill);
            UnityEngine.Debug.Log("Od ResultsManager jumpData.CPUDistance przed zmiana skilla: " + distance);
            if (skill == 90)
            {
                return distance;
            }
            else if (skill > 90)
            {
                UnityEngine.Debug.Log("Gra widzi, ze skill jest wyzszy niz 90");
                modifier += (((float)skill - 90f) / 110f);
                UnityEngine.Debug.Log("Od ResultsManager CPU Distance modifeir: " + modifier);

            }
            else if (skill < 90)
            {
                UnityEngine.Debug.Log("Gra widzi, ze skill jest nizszy niz 90");
                modifier -= ((90f - (float)skill) / 110f);
                UnityEngine.Debug.Log("Od ResultsManager CPU Distance modifeir: " + modifier);
            }


          //  return distance * (decimal)modifier;
          return (decimal)Math.Round(distance * (decimal)modifier, MidpointRounding.AwayFromZero) / 2;
        }

    }
}
