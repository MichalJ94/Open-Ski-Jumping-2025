using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenSkiJumping.Scripts2025
{
    public class Scripts2005
    {
        // Start is called before the first frame update
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
