using System;
using System.Collections;
using System.Collections.Generic;
using OpenSkiJumping.Competition;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.Competition.Runtime;
using OpenSkiJumping.Data;
using OpenSkiJumping.TVGraphics.SideResults;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using EventType = OpenSkiJumping.Competition.EventType;

namespace OpenSkiJumping.TVGraphics
{
    [Serializable]
    public class GraphicsData
    {
        public string name;
        public GameObject master;
        public PreJumpUIManager preJump;
        public SpeedUIManager speed;
        public ToBeatUIManager toBeat;
        public PostJumpUIManager postJump;
        public SideResultsController sideResults;
        public RoundResultsController roundResults;
        public CompetitionRunner competitionRunner;
    }

    public class TvGraphicsController : MonoBehaviour
    {
        [SerializeField] private SavesRuntime savesRepository;
        [SerializeField] private TMP_Text hillNameText;
        
        public int current;
        public float preJumpGraphicsCooldown;
        public float postJumpGraphicsCooldown;
        public RuntimeResultsManager resultsManager;
        public RuntimeCompetitorsList competitors;
        public RoundResultsController roundResultsController;
        public List<GraphicsData> graphicsData;
        private bool sideResultsActive;
        public UnityEvent onRoundCompleted;


        private EventInfo currentEvent;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                graphicsData[current].preJump.Show();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                graphicsData[current].postJump.Show();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                graphicsData[current].preJump.Hide();
                graphicsData[current].postJump.Hide();
            }

            if(roundResultsController.listViewAccessibleItems.Count == 50)
            {
                onRoundCompleted.Invoke();
            }


           if (Input.GetKeyDown(KeyCode.Tab))
            {
                Debug.Log("graphicsData[current].sideResults.gameObject.transform.localScale = " + graphicsData[current].sideResults.gameObject.transform.localScale);
                if (sideResultsActive == false)
                    {
                    graphicsData[current].sideResults.gameObject.transform.localScale = new Vector3(1, 1, 1);
                    sideResultsActive = true;
                        }
                else
                {
                    graphicsData[current].sideResults.gameObject.transform.localScale = new Vector3(0, 0, 0);
                    sideResultsActive = false;
                };
                //  graphicsData[current].sideResults.gameObject
                //    .SetActive(!graphicsData[current].sideResults.gameObject.activeSelf);
            }
        }

        public void OnCompetitionStart()
        {
            var save = savesRepository.GetCurrentSave();
            currentEvent = save.calendar.events[save.resultsContainer.eventIndex];

            if (currentEvent.eventType == EventType.Individual)
            {
                current = 0;
            }
            else if (currentEvent.eventType == EventType.Team)
            {
                current = 1;
            }
        
            SetMastersActive();
            hillNameText.text = currentEvent.hillId;
        }

        private void SetMastersActive()
        {
            for (var i = 0; i < graphicsData.Count; i++)
                graphicsData[i].master.SetActive(i == current);
        }

        public void UpdateListView()
        {
            graphicsData[current].sideResults.AddResult();
            graphicsData[current].roundResults.AddResult();
            // int competitorId = resultsManager.Value.StartList[resultsManager.Value.StartListIndex];
            // int bib = resultsManager.Value.Results[competitorId].Bibs[resultsManager.Value.RoundIndex];
            // int rank = resultsManager.Value.Results[competitorId].Rank;
            // Team team = competitors.teams[resultsManager.Value.Participants[competitorId].id];
            // listView.Items.Add(new ResultData { firstName = team.teamName, lastName = "", result = (float)resultsManager.Value.Results[competitorId].TotalPoints });
            // listView.Items = listView.Items.OrderByDescending(item => item.result).ToList();
        }

        public void ClearListView()
        {

    /*        graphicsData[current].roundResults.listViewItemsSnapshot =
    new List<int>(graphicsData[current].roundResults.listViewAccessibleItems);*/
            Debug.Log("ROUND RESULTS MISSION. Clear roundResults w TVGrphicsController.");
           graphicsData[current].sideResults.Clear();
           graphicsData[current].roundResults.Clear();
            graphicsData[current].roundResults.ClearSnapshot();
        }


        public void ShowPreJump()
        {
            graphicsData[current].preJump.Show();
        }

        public void HidePreJump()
        {
            StartCoroutine(HidePreJumpRoutine());
        }

        public void ShowPostJump()
        {
            StartCoroutine(ShowPostJumpRoutine());
        }

        public void HidePostJump()
        {
            graphicsData[current].postJump.InstantHide();
        }

        public void ShowToBeat()
        {
            graphicsData[current].toBeat.Show();
        }

        public void HideToBeat()
        {
            graphicsData[current].toBeat.Hide();
        }

        public void ShowSpeed()
        {
            graphicsData[current].speed.Show();
        }

        public void HideSpeed()
        {
            graphicsData[current].speed.Hide();
        }

        private IEnumerator HidePreJumpRoutine()
        {
            yield return new WaitForSeconds(preJumpGraphicsCooldown);
            graphicsData[current].preJump.Hide();
        }

        private IEnumerator ShowPostJumpRoutine()
        {
            yield return new WaitForSeconds(postJumpGraphicsCooldown);
            graphicsData[current].postJump.Show();
        }
    }
}