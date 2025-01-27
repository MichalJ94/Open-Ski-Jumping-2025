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

        public TMP_Text resultT;
        public TMP_Text secondStyle;
        public TMP_Text secondDistance;
        public TMP_Text secondGateT;
        public TMP_Text secondWindT;
        public TMP_Text firstStyle;
        public TMP_Text firstDistance;
        public TMP_Text firstGateT;
        public TMP_Text firstWindT;
        public TMP_Text rankChangeT;

        public TranslatablePhrase result;
        public TranslatablePhrase dist;
        public TranslatablePhrase styl;
        public TranslatablePhrase gate;
        public TranslatablePhrase wind;
        public TranslatablePhrase firstDist;
        public TranslatablePhrase secondDist;
        public TranslatablePhrase firstStyl;
        public TranslatablePhrase secondStyl;
        public TranslatablePhrase firstGate;
        public TranslatablePhrase secondGate;
        public TranslatablePhrase firstWind;
        public TranslatablePhrase secondWind;
        public TranslatablePhrase previousRoundRank;
        public TranslatablePhrase rankChange;
        public TranslatablePhrase windGateComp;

        public void Initialize()
        {
            int roundIndex = resultsManager.Value.RoundIndex;
            resultT.text = result.CurrentValue;
            Debug.Log($"RoundResultsHeader Initialzie resultsManager.Value.RoundIndex: {resultsManager.Value.RoundIndex}, resultsManager.Value.EventInfo.roundInfos.Count: {resultsManager.Value.EventInfo.roundInfos.Count}");

            if (roundIndex != resultsManager.Value.EventInfo.roundInfos.Count && resultsManager.Value.EventInfo.roundInfos.Count < 3) 
            {

                Debug.Log("RoundResultsHeader resultsManager.Value.RoundIndex != (resultsManager.Value.EventInfo.roundInfos.Count-1");


                firstDistance.enabled = false;
                firstStyle.enabled = false;
                firstGateT.enabled = false;
                firstWindT.enabled = false;
                rankChangeT.enabled = false;
                secondStyle.text = firstStyl.CurrentValue;
                secondDistance.text = firstDist.CurrentValue;
                secondGateT.text = firstGate.CurrentValue;
                secondWindT.text = firstWind.CurrentValue;
            }

            else

            {
                Debug.Log("RoundResultsHeader else");
                firstDistance.enabled = true;
                firstStyle.enabled = true;
                firstGateT.enabled = true;
                firstWindT.enabled = true;
                rankChangeT.enabled = true;
                firstDistance.text = firstDist.CurrentValue;
                firstStyle.text = result.CurrentValue + " " + (roundIndex - 1);
                firstGateT.text = windGateComp.CurrentValue + " " + (roundIndex -1);
                firstWindT.text = previousRoundRank.CurrentValue + " " + (roundIndex-1);
                secondStyle.text = secondStyl.CurrentValue;
                secondDistance.text = secondDist.CurrentValue;
                secondGateT.text = secondGate.CurrentValue + " " + roundIndex;
                secondWindT.text = secondWind.CurrentValue;
                rankChangeT.text = rankChange.CurrentValue;
            }

            if (resultsManager.Value.EventInfo.roundInfos.Count == 1)
            {

                Debug.Log("RoundResultsHeader resultsManager.Value.EventInfo.roundInfos.Count == 1");

                firstDistance.enabled = false;
                firstStyle.enabled = false;
                firstGateT.enabled = false;
                firstWindT.enabled = false;
                secondStyle.text = styl.CurrentValue;
                secondDistance.text = dist.CurrentValue;
                secondGateT.text = gate.CurrentValue;
                secondWindT.text = wind.CurrentValue;
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

