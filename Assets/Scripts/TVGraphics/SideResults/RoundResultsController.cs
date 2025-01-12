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
        public RoundResultsListView listViewAccessible;
        public List<int> listViewAccessibleItems;
        private int[] LastRankInController;
        private SortedList<(int state, decimal points, int bib), int> finalResultsGrabbed;
        public List<RoundResultsItemData> listViewItemsSnapshot = new List<RoundResultsItemData>();
        [SerializeField] protected RuntimeResultsManager resultsManager;
        [SerializeField] protected RuntimeCompetitorsList competitorsList;
        [SerializeField] protected List<int> listViewItems;
        [SerializeField] CompetitionRunner competitionRunner;


        private UnityAction onContinue;

        public void Start()
        {
            listViewItems = new List<int>();
            listView.SelectionType = SelectionType.None;
            listView.Items = listViewItems;
            listView.Initialize(BindListViewItem);

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }
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
            var localId = resultsManager.Value.GetIdByRank(index);
            var globalId = resultsManager.Value.OrderedParticipants[localId].id;
            var item = resultsManager.Value.Results[localId];
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

            // Fill in the snapshot
            var snapshotItem = new RoundResultsItemData
            {
                Rank = listItem.rankText.text,
                Name = listItem.nameText.text,
                CountryCode = listItem.countryFlagText.text,
                CountryFlag = listItem.countryFlagImage.sprite,
                TotalPoints = listItem.resultText.text,
                Distance = listItem.distanceText.text,
                Gate = listItem.gateText.text,
                PreviousRoundDistance = listItem.previousRoundDistanceText.text,
                Style = listItem.styleText.text,
                PreviousRoundStyle = listItem.previousRoundStyleText.text,
                ShowPreviousRoundDistance = listItem.previousRoundDistanceText.enabled,
                ShowPreviousRoundStyle = listItem.previousRoundStyleText.enabled
            };

            // Add logging to debug snapshot addition
          //  Debug.Log($"Checking snapshot: Rank={snapshotItem.Rank}, Name={snapshotItem.Name}");
            if (!listViewItemsSnapshot.Any(snapshot => snapshot.Name == snapshotItem.Name))
            {
               listViewItemsSnapshot.Add(snapshotItem);
                //Debug.Log($"Snapshot added: Rank={snapshotItem.Rank}, Name={snapshotItem.Name}");
            }
            else
            {
             // Debug.LogWarning($"Duplicate or missing name detected: {snapshotItem.Name}");
            }

            /*
            if (resultsManager.Value.LastRank[0] != 0)
            {
                listViewAccessibleItems = new List<int>(listViewItems);
                LastRankInController = resultsManager.Value.LastRank.ToArray();
                Debug.Log($"LastRankInController.Length: {LastRankInController.Length}");
                if(LastRankInController.Length == competitionRunner.bibColors)
                {
                    PrintRoundResults();
                }
                // Creates a copy of the list.
                /* Debug.Log("listViewAccessibleItems = listViewItems. listViewItems.Count: " + listViewItems.Count +
                           " listView.Items.Count: " + listView.Items.Count +
                           " listViewItemsSnapshot.Count: " + listViewItemsSnapshot.Count);
                 */
        
    }

        public void GrabRoundResults()
        {
            

            finalResultsGrabbed = resultsManager.Value.GetFinalResultsAccessible();

            Debug.Log($"Essa essa pobra³em wyniniki tej rundy. Spróbujmy je wydrukowaæ. Ale najpierw finalResultsGrabbed.Count: {finalResultsGrabbed.Count}");
            // PrintRoundResults();
        }

        public void PrintGrabbedFinalResults()
        {

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
        public void PopulateSnapshotFromListViewItems()
        {
            // Clear the snapshot to avoid duplicates
            listViewItemsSnapshot.Clear();

            foreach (var index in listViewItems)
            {
                var localId = resultsManager.Value.GetIdByRank(index);
                var globalId = resultsManager.Value.OrderedParticipants[localId].id;
                var item = resultsManager.Value.Results[localId];

                var snapshotItem = new RoundResultsItemData
                {
                    Rank = $"{item.Rank}",
                    Name = $"{GetNameById(globalId)}",
                    CountryCode = $"{GetCountryCodeById(globalId)}",
                    CountryFlag = flagsData.GetFlag(GetCountryCodeById(globalId)),
                    TotalPoints = $"{item.TotalPoints.ToString("F1", CultureInfo.InvariantCulture)}",
                    Distance = $"{item.Distance.ToString("F1", CultureInfo.InvariantCulture)} m",
                    Gate = $"{item.ActualGate}",
                    PreviousRoundDistance = $"{item.PreviousRoundDistance.ToString("F1", CultureInfo.InvariantCulture)} m",
                    Style = item.Style > 0 ? $"{item.Style.ToString("F1", CultureInfo.InvariantCulture)}" : "",
                    PreviousRoundStyle = item.PreviousRoundStyle > 0 && item.PreviousRoundStyle <= 60
                        ? $"{item.PreviousRoundStyle.ToString("F1", CultureInfo.InvariantCulture)}"
                        : "",
                    ShowPreviousRoundDistance = item.PreviousRoundDistance > 0,
                    ShowPreviousRoundStyle = item.PreviousRoundStyle > 0 && item.PreviousRoundStyle <= 60
                };

                listViewItemsSnapshot.Add(snapshotItem);

                Debug.Log($"Snapshot added: Rank={snapshotItem.Rank}, Name={snapshotItem.Name}");
            }

            Debug.Log($"Final snapshot count: {listViewItemsSnapshot.Count}");
        }
        public void PrintSortedSnapshotItems()
        {
            // Sort the snapshot items by TotalPoints in descending order
            var sortedSnapshot = listViewItemsSnapshot
                .OrderByDescending(snapshot =>
                {
                    if (float.TryParse(snapshot.TotalPoints, NumberStyles.Float, CultureInfo.InvariantCulture, out float totalPoints))
                        return totalPoints;
                    return 0f; // Default value if parsing fails
                })
                .ToList();

            Debug.Log("===== Sorted Snapshot Items by Total Points =====");

            // Print each item with its place
            for (int place = 0; place < sortedSnapshot.Count; place++)
            {
                var item = sortedSnapshot[place];
                Debug.Log($"Place: {place + 1}, Name: {item.Name}, Total Points: {item.TotalPoints}, Rank: {item.Rank}");
            }
            Debug.Log("=================================================");
        }


        public void LogSortedSnapshotByPoints()
        {
            var sortedSnapshot = listViewItemsSnapshot
                .OrderByDescending(item => float.Parse(item.TotalPoints, CultureInfo.InvariantCulture))
                .ToList();

            for (int i = 0; i < sortedSnapshot.Count; i++)
            {
                Debug.Log($"Place {i + 1}: Name={sortedSnapshot[i].Name}, TotalPoints={sortedSnapshot[i].TotalPoints}");
            }
        }

        protected abstract string GetNameById(int id);
        protected abstract string GetCountryCodeById(int id);

        public void Clear()
        {
            listViewItems.Clear();
            listView.Refresh();
        }

        public void ClearSnapshot()
        {
            listViewItemsSnapshot.Clear();
            
        }


        public void AddResult()
        {
            listViewItems.Add(0);
            listView.Refresh();
        }

        public void PopulateList()
        {
            listView.Items = listViewAccessibleItems;
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