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
        [SerializeField] protected List<int> listViewItems;
        [SerializeField] CompetitionRunner competitionRunner;


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

        private void BindListViewItem(int index, RoundResultsListItem listItem)
        {
            Debug.Log($"From BindListViewItem resultsManager.Value.ResultsDeepCopy[index].CurrentCompetitorId {resultsManager.Value.ResultsDeepCopy[index].CurrentCompetitorId}");
            var globalId = resultsManager.Value.ResultsDeepCopy[index].CurrentCompetitorId;
            var item = resultsManager.Value.ResultsDeepCopy[index];
            var roundNumber = resultsManager.Value.GetRoundNumber();

            // Fill in the listItem UI components
            listItem.rankText.text = $"{item.Rank}";
            listItem.nameText.text = $"{GetNameById(globalId)}";
            listItem.countryFlagText.text = $"{GetCountryCodeById(globalId)}";
            listItem.countryFlagImage.sprite = flagsData.GetFlag(GetCountryCodeById(globalId));
            listItem.resultText.text = $"{item.TotalPoints.ToString("F1", CultureInfo.InvariantCulture)}";
            listItem.distanceText.text = $"{item.Distance.ToString("F1", CultureInfo.InvariantCulture)} m";
            listItem.gateText.text = $"{item.ActualGate}";
            listItem.previousRoundDistanceText.text = $"{item.PreviousRoundDistance.ToString("F1", CultureInfo.InvariantCulture)}";

            if (roundNumber == 0)
            {
                listItem.previousRoundDistanceText.enabled = false;
                listItem.previousRoundStyleText.enabled = false;
            }
            else
            {
                listItem.previousRoundDistanceText.enabled = true;
                listItem.previousRoundStyleText.enabled = true;
            }

            listItem.styleText.text = item.Style > 0
                ? $"{item.Style.ToString("F1", CultureInfo.InvariantCulture)}"
                : "";

            listItem.previousRoundStyleText.text = item.PreviousRoundStyle > 0 && item.PreviousRoundStyle <= 60
                ? $"{item.PreviousRoundStyle.ToString("F1", CultureInfo.InvariantCulture)}"
                : "";


        
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


               //Debug.Log($"resultsManager.Value.ResultsDeepCopy[i].Rank = {(resultsManager.Value.ResultsDeepCopy[i].Rank)-1} resultsManager.Value.ResultsDeepCopy[i].TotalPoints: {resultsManager.Value.ResultsDeepCopy[i].TotalPoints}");
               Debug.Log($"resultsManager.Value.ResultsDeepCopy[i].Rank = {(resultsManager.Value.ResultsDeepCopy[i].Rank)} resultsManager.Value.ResultsDeepCopy[i].TotalPoints: {resultsManager.Value.ResultsDeepCopy[i].TotalPoints} resultsManager.Value.ResultsDeepCopy[i].CurrentCompetitorId {resultsManager.Value.ResultsDeepCopy[i].CurrentCompetitorId} IName: {GetNameById(resultsManager.Value.ResultsDeepCopy[i].CurrentCompetitorId)}");
                Initialize();
                PopulateTheList();
                
                //Debug.Log($"resultsManager.Value.IDDeepCopy[i].Item2: {resultsManager.Value.IDDeepCopy[i].Item2} Name: {GetNameById(resultsManager.Value.IDDeepCopy[i].Item2)}");
            
                           
            }
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
                listView.Refresh();
            }

            // Refresh the list view

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