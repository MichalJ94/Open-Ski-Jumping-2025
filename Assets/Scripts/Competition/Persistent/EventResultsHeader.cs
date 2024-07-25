using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using OpenSkiJumping.Competition;
using OpenSkiJumping.Competition.Persistent;
using TMPro;
using UnityEngine;

namespace OpenSkiJumping.UI.TournamentMenu.ResultsMenu
{
    public class EventResultsHeader : MonoBehaviour
    {
        [SerializeField] private EventsSelectionView eventsSelectionView;
        public TMP_Text secondStyle;
        public TMP_Text secondDistance;
        public TMP_Text firstStyle;
        public TMP_Text firstDistance;




        private void Start()
        { 
            eventsSelectionView.OnSelectionChanged += eventsSelectionView.OnNewEventSelected;
        }




        public void UpdateAccordingToSelectedEvent(EventInfo item)
        {

            if (item.roundInfos.name.Contains("Trial"))
            {
                firstDistance.enabled = false;
                firstStyle.enabled = false;
                secondDistance.enabled = true;
                secondDistance.text = "Dist";
                secondStyle.enabled = false;
            }
            else if (item.roundInfos.name.Contains("Qual"))
            {
                firstDistance.enabled = false;
                firstStyle.enabled = false;
                secondDistance.enabled = true;
                secondDistance.text = "Dist";
                secondStyle.enabled = true;
                secondStyle.text = "Style";
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
                firstStyle.enabled = true;
                secondDistance.enabled = true;
                secondDistance.text = "2nd Dist";
                secondStyle.enabled = true;
                secondStyle.text = "2nd Style";
            }

        }


    }

}

