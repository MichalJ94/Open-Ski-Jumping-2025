using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using OpenSkiJumping.Competition;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.Competition.Runtime;
using OpenSkiJumping.ScriptableObjects;
using TMPro;
using UnityEngine;

namespace OpenSkiJumping.UI.TournamentMenu.ResultsMenu
{
    public class RoundResultsHeader : MonoBehaviour
    {

        public RuntimeResultsManager resultsManager;

        public TMP_Text secondStyle;
        public TMP_Text secondDistance;
        public TMP_Text firstStyle;
        public TMP_Text firstDistance;
        public TMP_Text firstGateT;
        public TMP_Text secondGateT;
        public TMP_Text firstWindT;
        public TMP_Text secondWindT;

        public TranslatablePhrase result;
        public TranslatablePhrase dist;
        public TranslatablePhrase styl;
        public TranslatablePhrase firstDist;
        public TranslatablePhrase secondDist;
        public TranslatablePhrase firstStyl;
        public TranslatablePhrase secondStyl;
        public TranslatablePhrase firstGate;
        public TranslatablePhrase secondGate;
        public TranslatablePhrase firstWind;
        public TranslatablePhrase secondWind;

        public void Initialize()
        {

            if(resultsManager.Value.RoundIndex != resultsManager.Value.EventInfo.roundInfos.Count) 
            
            {
                Debug.Log("RoundResultsHeader Initialzie");

                firstDistance.enabled = false;
                firstStyle.enabled = false;
                firstGateT.enabled = false;
                firstWindT.enabled = false;

            }

            else

            {

                firstDistance.enabled = true;
                firstStyle.enabled = true;
                firstGateT.enabled = true;
                firstWindT.enabled = true;

            }


        }



        /*
                public void UpdateAccordingToSelectedEvent(EventInfo item)
                {

                    if (item.roundInfos.name.Contains("Trial"))
                    {
                        firstDistance.enabled = false;
                        firstStyle.enabled = false;
                        secondDistance.enabled = true;
                        secondDistance.text = dist.CurrentValue;
                        secondStyle.enabled = false;
                    }
                    if (item.roundInfos.name.Contains("Qual"))
                    {
                        firstDistance.enabled = false;
                        firstStyle.enabled = false;
                        secondDistance.enabled = true;
                        secondDistance.text = dist.CurrentValue;
                        secondStyle.enabled = true;
                        secondStyle.text = styl.CurrentValue;

                    }
                    else if (item.eventType == Competition.EventType.Team)
                    {
                        firstDistance.enabled = false;
                        firstStyle.enabled = false;
                        secondDistance.enabled = false;
                        secondStyle.enabled = false;
                    }
                    else
                    {
                        firstDistance.enabled = true;
                        firstDistance.text = firstDist.CurrentValue;
                        firstStyle.enabled = true;
                        firstStyle.text = firstStyl.CurrentValue;
                        secondDistance.enabled = true;
                        secondDistance.text = secondDist.CurrentValue;
                        secondStyle.enabled = true;
                        secondStyle.text = secondStyl.CurrentValue;
                    }

                }*/


    }

}

