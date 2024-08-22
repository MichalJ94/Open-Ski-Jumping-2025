using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using OpenSkiJumping.Competition;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace OpenSkiJumping.UI.TournamentMenu.ResultsMenu
{
    public class ClassificationResultsHeader : MonoBehaviour
    {
        [SerializeField] private ClassificationsSelectionView classificationsSelectionView;
        [SerializeField] private ClassificationsSelectionPresenter classificationsSelectionPresenter;
        [SerializeField] public GameObject goldMedal;
        [SerializeField] public GameObject silverMedal;
        [SerializeField] public GameObject bronzeMedal;


        public TranslatablePhrase result;




        private void Start()
        {
            classificationsSelectionView.OnSelectionChanged += classificationsSelectionView.OnNewClasificationSelected;
        }




        public void UpdateAccordingToSelectedEvent(ClassificationInfo item)
        {

           if(item.eventType == Competition.EventType.Team)
            {
                goldMedal.active = false;
                silverMedal.active= false;
                bronzeMedal.active= false;

            }
            else
            {
                goldMedal.active = true;
                silverMedal.active= true;
                bronzeMedal.active= true;
            }
        }


    }

}

