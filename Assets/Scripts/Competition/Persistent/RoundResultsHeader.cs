using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using OpenSkiJumping.Competition;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.ScriptableObjects;
using TMPro;
using UnityEngine;

namespace OpenSkiJumping.UI.TournamentMenu.ResultsMenu
{
    public class RoundResultsHeader : MonoBehaviour
    {
        public TMP_Text secondStyle;
        public TMP_Text secondDistance;
        public TMP_Text firstStyle;
        public TMP_Text firstDistance;


        public TranslatablePhrase result;
        public TranslatablePhrase dist;
        public TranslatablePhrase styl;
        public TranslatablePhrase firstDist;
        public TranslatablePhrase secondDist;
        public TranslatablePhrase firstStyl;
        public TranslatablePhrase secondStyl;



        private void Start()
        {
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

