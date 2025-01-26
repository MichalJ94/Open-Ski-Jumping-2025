using System.Collections.Generic;
using System.Globalization;
using OpenSkiJumping.Competition.Runtime;
using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.UI.ListView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using OpenSkiJumping.Competition;
using System.Linq;
using UnityEngine.SocialPlatforms;
using OpenSkiJumping.UI.TournamentMenu.ResultsMenu;
using static UnityEditor.Progress;

namespace OpenSkiJumping.TVGraphics.SideResults
{
    public abstract class RoundResultsController : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] protected FlagsData flagsData;
        [SerializeField] protected RoundResultsListView listView;
        //public RoundResultsListView listViewAccessible;
        //public List<int> listViewAccessibleItems;
        private int[] LastRankInController;
        private SortedList<(int state, decimal points, int bib), int> finalResultsGrabbed;
        [SerializeField] protected RuntimeResultsManager resultsManager;
        [SerializeField] protected RuntimeCompetitorsList competitorsList;
        [SerializeField] protected RoundResultsHeader roundResultsHeader;
        [SerializeField] protected List<int> listViewItems;
        [SerializeField] CompetitionRunner competitionRunner;
        private Dictionary<int, RoundResultData> previousRoundDataStored = new Dictionary<int, RoundResultData>();

        private UnityAction onContinue;

        public void Start()
        {
            listViewItems = new List<int>();
            listView.SelectionType = SelectionType.None;


            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        public void Initialize()
        {

            roundResultsHeader.Initialize();

            if (resultsManager.Value.ResultsDeepCopy == null || resultsManager.Value.ResultsDeepCopy.Length < competitionRunner.bibColors)
            {
                Debug.LogWarning("Results data is not yet available. Delaying list initialization.");
                return;
            }

            if (listView == null)
            {
                Debug.LogError("ListView is not assigned. Check the Inspector.");
                return;
            }

            Debug.Log("Now I initialize  the Round Results listview");
            listView.Items = listViewItems ?? new List<int>(); // Ensure listViewItems is not null
            listView.Initialize(BindListViewItem);
        }

        public void Show()
        {
            gameObject.transform.localScale = new Vector3(1, 1, 1);
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }

        private void OnContinueClicked()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            gameObject.transform.localScale = new Vector3(0, 0, 0);
            onContinue?.Invoke();
        }

        public void Hide()
        {
            Time.timeScale = 1;
            AudioListener.pause = false;
            gameObject.transform.localScale = new Vector3(0, 0, 0);
        }


        /*
        ******************************************************************************************
        NAMING CONVENTION GUIDE
        
        While the Round Results Controller and scoreboard are being adjusted, variables stored within RoundResultsListItem and Results class
        can be used to display other information in the actual scorebaord than what their names indicate. Therefore, this guide shall serve
        a quick reference for what data respective variables actually store and display in the game.



        PreviousRoundGate -> Wind and Gate Compensation in the previous round
        PreviousRoundWind -> Previous Round Rank
        PreviousRoundStyle -> Previous Round TotalPoints



         ******************************************************************************************
        */

        private void BindListViewItem(int index, RoundResultsListItem listItem)
        {
            //Debug.Log($"From BindListViewItem resultsManager.Value.ResultsDeepCopy[index].CurrentCompetitorId {resultsManager.Value.ResultsDeepCopy[index].CurrentCompetitorId}");
            var globalId = resultsManager.Value.ResultsDeepCopy[index].CurrentCompetitorId;
            var item = resultsManager.Value.ResultsDeepCopy[index];
            var roundNumber = resultsManager.Value.GetRoundNumber();

            // Fill in the listItem UI components

            //item.Results[index].results[index].ffff - prior rounds access

            listItem.rankText.text = $"{item.Rank}";
            listItem.nameText.text = $"{GetNameById(globalId)}";
            listItem.countryFlagText.text = $"{GetCountryCodeById(globalId)}";
            listItem.countryFlagImage.sprite = flagsData.GetFlag(GetCountryCodeById(globalId));
            listItem.resultText.text = $"{item.TotalPoints.ToString("F1", CultureInfo.InvariantCulture)}";
            listItem.distanceText.text = $"{item.Distance.ToString("F1", CultureInfo.InvariantCulture)} m";
            listItem.gateText.text = $"{item.ActualGate}";
            listItem.windText.text = $"{item.Wind.ToString("F1", CultureInfo.InvariantCulture)}";


            if(roundNumber != 0)
            {
                //Code to print previous round rank. Let's try!
                //Debug.Log($"Od BindListViewItem name: {listItem.nameText.text} previous round Rank:");
            }

            if (roundNumber == 0)
            {
                listItem.previousRoundDistanceText.enabled = false;
                listItem.previousRoundStyleText.enabled = false;
                listItem.previousRoundGateText.enabled = false;
                listItem.previousRoundWindText.enabled = false;
                listItem.rankChange.enabled = false;
            }
            else
            {
                listItem.previousRoundDistanceText.enabled = true;
                listItem.previousRoundStyleText.enabled = true;
                listItem.previousRoundGateText.enabled = true;
                listItem.previousRoundWindText.enabled = true;
                listItem.rankChange.enabled = true;
            }

            listItem.styleText.text = item.Style > 0
                ? $"{item.Style.ToString("F1", CultureInfo.InvariantCulture)}"
                : "";

            listItem.previousRoundStyleText.text = previousRoundDataStored[item.CurrentCompetitorId].TotalPoints > 0
                ? $"{previousRoundDataStored[item.CurrentCompetitorId].TotalPoints.ToString("F1", CultureInfo.InvariantCulture)}"
                : "";

            /*listItem.previousRoundGateText.text = item.PreviousRoundGate > 0
                ? $"{item.PreviousRoundGate}"
                : "";*/
            listItem.previousRoundGateText.text = item.PreviousRoundGate > 0
                ? $"{previousRoundDataStored[item.CurrentCompetitorId].WindGateComp}"
                : "";


            listItem.previousRoundWindText.text = previousRoundDataStored.ContainsKey(item.CurrentCompetitorId)
                ? $"{previousRoundDataStored[item.CurrentCompetitorId].Rank}"
                : "";

            listItem.previousRoundDistanceText.text = item.PreviousRoundDistance > 0
                ? $"{item.PreviousRoundDistance.ToString("F1", CultureInfo.InvariantCulture)} m"
                : "";

            int rankChange = previousRoundDataStored[item.CurrentCompetitorId].Rank - item.Rank;

            // Set the text for rank change
            listItem.rankChange.text = item.Rank < resultsManager.Value.Results.Count()
                ? $"{rankChange}"
                : "";


            // Change the text color based on the rank change
            if (rankChange > 0)
            {
                listItem.rankChange.color = Color.green; // Positive rank change - green
            }
            else if (rankChange < 0)
            {
                listItem.rankChange.color = Color.red; // Negative rank change - red
            }
            else
            {
                listItem.rankChange.color = Color.white; // No rank change - white or default
            }

            //  $"{item.PreviousRoundDistance.ToString("F1", CultureInfo.InvariantCulture)}";




        }

        public void GrabRoundResults()
        {

           
            finalResultsGrabbed = resultsManager.Value.GetFinalResultsAccessible();

            Debug.Log($"Essa essa pobra³em wyniniki tej rundy. Spróbujmy je wydrukowaæ. Ale najpierw finalResultsGrabbed.Count: {finalResultsGrabbed.Count} resultsManager.Value.Results.Length: {resultsManager.Value.Results.Length}");
            PrintGrabbedFinalResults();
            //PopulateTheList();
        }

        public void PrintGrabbedFinalResults()
        {
            //for (int i = 0; i < finalResultsGrabbed.Count; i++)
            for (int i = 0; i < resultsManager.Value.ResultsDeepCopy.Length; i++)
            {


                //var localId = resultsManager.Value.GetIdByRank(resultsManager.Value.ResultsDeepCopy[i].Rank);
                //var globalId = resultsManager.Value.OrderedParticipants[localId].id;


                //Problemy ponizej w konkursie 4-seryjnym
                Debug.Log($"resultsManager.Value.ResultsDeepCopy[i].Rank = {(resultsManager.Value.ResultsDeepCopy[i].Rank)}   Name: {GetNameById(resultsManager.Value.ResultsDeepCopy[i].CurrentCompetitorId)}");

            }

            //HERE I WANT TO STORE Rank, TotalPoints and currentCompetitorId.
            if (resultsManager.Value.GetRoundNumber() < (resultsManager.Value.EventInfo.roundInfos.Count-1))
              {


                int roundNumber = resultsManager.Value.GetRoundNumber();
                  previousRoundDataStored.Clear();
                //Attempt to replace ResultsDeepCopy.Count with competitionRunner.bibColors
                for (int k = 0; k < resultsManager.Value.ResultsDeepCopy.Length; k++)
                  {
                  //  Debug.Log($"Before adding previousRoundData stored item. k: {k} roundNumber {roundNumber}");
                      var item = resultsManager.Value.ResultsDeepCopy[k];
                  //  Debug.Log($" var item = resultsManager.Value.ResultsDeepCopy[k], k: {k} CurrentCompetitorId {item.CurrentCompetitorId} ");

                //Up until this point, the code is executed correctly.
                    previousRoundDataStored[item.CurrentCompetitorId] = new RoundResultData
                      {
                          Rank = item.Rank,
                          TotalPoints = item.TotalPoints,
                        //WindGateComp messing up the game in comps with more than 2 rounds if they have more than 30 jumpers!!   
                        WindGateComp = item.Results[0].results[roundNumber].windPoints + item.Results[0].results[roundNumber].gatePoints
                      };

                     //   Debug.Log($"Debugowanie. item.Results[0].results[roundNumber].windPoints + item.Results[0].results[roundNumber].gatePoints {item.Results[0].results[roundNumber].windPoints + item.Results[0].results[roundNumber].gatePoints}"); 

                  }
              }

            Initialize();
            PopulateTheList();
            Show();
        }

        private void PopulateTheList()
        {
            if (listView == null)
            {
                Debug.LogError("ListView is not assigned.");
                return;
            }

            if (resultsManager?.Value?.ResultsDeepCopy == null)
            {
                Debug.LogWarning("Results data is not available. Skipping PopulateTheList.");
                return;
            }

            // Clear existing items
            listView.Items.Clear();

            // Populate the list
            for (int i = 0; i < resultsManager.Value.ResultsDeepCopy.Length; i++)
            {
                listView.Items.Add(i); // Add indices as keys
                if (i == (resultsManager.Value.ResultsDeepCopy.Length) - 1)
                {
                    listView.Refresh();
                }
            }

        }

        public void PrintRoundResults()
        {
            for(int i = 1; i <= LastRankInController.Length; i++)
            {
                Debug.Log($"PrintRoundResults i = {i}: wartosc i = {LastRankInController[i]}");
                
                var localId = resultsManager.Value.GetIdByRank(LastRankInController[i]);
                var globalId = resultsManager.Value.OrderedParticipants[localId].id;
                var item = resultsManager.Value.Results[localId];

              //  Debug.Log($"PrintRoundResults pozycja = {i+1}: {GetNameById(globalId)} localID = {localId} globalID = {globalId}");
            }
        }
       

        protected abstract string GetNameById(int id);
        protected abstract string GetCountryCodeById(int id);

        public void Clear()
        {
            listViewItems.Clear();
            listView.Refresh();
        }



        public void AddResult()
        {
            listViewItems.Add(0);
            listView.Refresh();
        }


    }

    public class RoundResultsItemData
    {
        public string Rank { get; set; }
        public string Name { get; set; }
        public string CountryCode { get; set; }
        public Sprite CountryFlag { get; set; }
        public string TotalPoints { get; set; }
        public string Distance { get; set; }
        public string Gate { get; set; }
        public string PreviousRoundDistance { get; set; }
        public string Style { get; set; }
        public string PreviousRoundStyle { get; set; }
        public bool ShowPreviousRoundDistance { get; set; }
        public bool ShowPreviousRoundStyle { get; set; }
    }
}

public class RoundResultData
{
    public int Rank { get; set; }
    public decimal TotalPoints { get; set; }

    public decimal WindGateComp { get; set; }
}