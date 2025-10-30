using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.UI.JumpersMenu;
using OpenSkiJumping.UI.ListView;
using OpenSkiJumping.UI.TournamentMenu.ResultsMenu;
using OpenSkiJumping.Competition;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace OpenSkiJumping.UI.TournamentMenu.JumpersSelection
{
    public class IndResJumpersSelectionView : MonoBehaviour, IIndResJumpersSelectionView
    {
        private bool initialized;
        private IndResJumpersSelectionPresenter presenter;

        [SerializeField] private TournamentMenuData tournamentMenuData;
        [SerializeField] private FlagsData flagsData;
        [SerializeField] private IconsData iconsData;
        [SerializeField] private IndResultsListController indResultsListController;
        [SerializeField] private Toggle includeQualiTrialToggle;

        [SerializeField] private JumpersSelectionListView listView;

        public CompetitorData SelectedJumper 
        {
            get => listView.SelectedIndex < 0 ? null : jumpers [listView.SelectedIndex];
            set => SelectJumper(value); 
        }

        private List<CompetitorData> jumpers;

        public IEnumerable<CompetitorData> Jumpers
        {
            set
            {
                jumpers = value.ToList();
                listView.Items = jumpers;
                listView.ClampSelectedIndex();
                listView.Refresh();
            }
        }
        private List<IndResultsListItem> results;

        private bool includeQualiTrial = false;

        public event Action OnDataReload;
        public event Action<CompetitorData> OnJumperSelected;
        public event Action OnSelectionChanged;

        private void Start()
        {
            ListViewSetup();
            indResultsListController.Initialize();

            presenter = new IndResJumpersSelectionPresenter(this, tournamentMenuData);
            initialized = true;

            // Listen to toggle changes
            includeQualiTrialToggle.onValueChanged.AddListener(OnQualiTrialToggleChanged);

            // Automatically select the first jumper after data is loaded
            StartCoroutine(AutoSelectFirstJumperNextFrame());
        }

        private void OnEnable()
        {
            if (!initialized) return;
            OnDataReload?.Invoke();
            listView.Reset();
        }
        public void Initialize()
        {
            listView.SelectionType = SelectionType.None;
            listView.Initialize(BindListViewItem);
        }

        private void BindListItem(int index, IndResultsListItemUI uiItem)
        {
            var data = results[index];
            uiItem.nameText.text = data.name;
            uiItem.rankText.text = data.rank > 0 ? data.rank.ToString() : " - ";
            uiItem.countryCodeText.text = data.countryCode;
            uiItem.valueText.text = data.rank > 0 ? data.value.ToString("F1") : " - ";
            uiItem.countryFlagImage.sprite = flagsData.GetFlag(data.countryCode);
        }

        private void ListViewSetup()
        {
            listView.OnSelectionChanged += HandleJumperSelected;
            listView.OnSelectionChanged += x => OnSelectionChanged?.Invoke();
            listView.SelectionType = SelectionType.Single;
            listView.Initialize(BindListViewItem);
        }

        private void BindListViewItem(int index, JumpersListItem listItem)
        {
            var item = jumpers[index];

            listItem.nameText.text = $"{item.competitor.firstName} {item.competitor.lastName.ToUpper()}";
            listItem.countryFlagText.text = item.competitor.countryCode;
            listItem.countryFlagImage.sprite = flagsData.GetFlag(item.competitor.countryCode);
            listItem.genderIconImage.sprite = iconsData.GetGenderIcon(item.competitor.gender);

        }

        private void HandleSelectionChanged(int index, bool value)
        {
            var item = jumpers[index];
        }
        /*private void HandleJumperSelected(int index)
        {
            if (index < 0 || index >= jumpers.Count) return;

            var jumper = jumpers[index];
            SelectedJumper = jumper;

            OnJumperSelected?.Invoke(jumper); // Let presenter or controller listen to this

            // Example logic to fetch results:
            var jumperResults = GetResultsForJumper(jumper);
            indResultsListController.Results = jumperResults;
        }*/

        private void HandleJumperSelected(int index)
        {
            if (index < 0 || index >= jumpers.Count) return;

            var jumper = jumpers[index];
            SelectedJumper = jumper;

            OnJumperSelected?.Invoke(jumper);

            // ✅ Generate full result list for this jumper
            var jumperResults = GetResultsForJumper(jumper);
            indResultsListController.Results = jumperResults;

            Debug.Log($"Selected jumper: {jumper.competitor.firstName} {jumper.competitor.lastName} (calendarID: {jumper.calendarId})");
            Debug.Log($"Generated {jumperResults.Count} results for jumper.");
        }

        private List<IndResultsListItem> GetResultsForJumper(CompetitorData jumper)
        {
            var jumperResults = new List<IndResultsListItem>();
            var allEvents = tournamentMenuData.GameSave.calendar.events;
            var resultContainer = tournamentMenuData.GameSave.resultsContainer.eventResults;

            for (int i = 0; i < allEvents.Count; i++)
            {
                var eventInfo = allEvents[i];

                // Skip team competitions
                if (eventInfo.eventType == OpenSkiJumping.Competition.EventType.Team)
                    continue;

                bool isQualification = eventInfo.roundInfos.name.StartsWith("Q");
                bool isTrial = eventInfo.roundInfos.name.Contains("Trial");

                if (!includeQualiTrial && (isQualification || isTrial))
                    continue;

                string displayHillName = eventInfo.hillId;
                if (isQualification) displayHillName += " (Q)";
                else if (isTrial) displayHillName += " Trial";

                int rank = 0;
                decimal value = 0m;
                ResultBackgroundStyle style = ResultBackgroundStyle.NoResult;

                // Try to find jumper result
                if (i < resultContainer.Length)
                {
                    var result = resultContainer[i];
                    if (result != null && result.competitorIds != null && result.results != null)
                    {
                        int competitorIndex = result.competitorIds.IndexOf(jumper.calendarId);
                        if (competitorIndex >= 0 && competitorIndex < result.results.Count)
                        {
                            var compResult = result.results[competitorIndex];
                            rank = compResult.Rank;
                            value = compResult.TotalPoints;

                            // Determine background style from placement
                            if (isTrial)
                                style = ResultBackgroundStyle.Trial;
                            else if (isQualification)
                                style = ResultBackgroundStyle.Qualification;
                            else if (rank == 1)
                                style = ResultBackgroundStyle.Place1;
                            else if (rank == 2)
                                style = ResultBackgroundStyle.Place2;
                            else if (rank == 3)
                                style = ResultBackgroundStyle.Place3;
                            else if (rank > 3 && rank <= 30)
                                style = ResultBackgroundStyle.Top30;
                            else if (rank > 30 && rank <= 50)
                                style = ResultBackgroundStyle.Bottom20;
                            else
                                style = ResultBackgroundStyle.NoResult;
                        }
                    }
                }

                var item = new IndResultsListItem
                {
                    competitionID = isQualification ? "Q" : isTrial ? "T" : "",
                    hillName = displayHillName,
                    name = $"{jumper.competitor.firstName} {jumper.competitor.lastName}",
                    countryCode = jumper.competitor.countryCode,
                    rank = rank,
                    value = value,
                    backgroundStyle = style
                };

                jumperResults.Add(item);
            }

            // Assign competition numbers
            int compIndex = 1;
            foreach (var item in jumperResults)
            {
                if (item.competitionID == "")
                {
                    item.competitionID = compIndex.ToString();
                    compIndex++;
                }
            }

            return jumperResults;
        }



        private IEnumerator AutoSelectFirstJumperNextFrame()
        {
            yield return null; // Wait one frame to ensure UI is ready

            if (jumpers != null && jumpers.Count > 0)
            {
                listView.SelectedIndex = 0;
                HandleJumperSelected(0);
            }
        }

        private void OnQualiTrialToggleChanged(bool isOn)
        {
            includeQualiTrial = isOn;

            // Refresh current jumper’s result list
            if (SelectedJumper != null)
            {
                var jumperResults = GetResultsForJumper(SelectedJumper);
                indResultsListController.Results = jumperResults;
            }
        }


        private int GetJumperId(CompetitorData jumper)
        {
            return tournamentMenuData.GameSave.competitors.FindIndex(c =>
                c.competitor.firstName == jumper.competitor.firstName &&
                c.competitor.lastName == jumper.competitor.lastName);
        }
        private void SelectJumper(CompetitorData item)
        {
            listView.SelectedIndex =
                item == null ? listView.SelectedIndex : jumpers.IndexOf(item);

            listView.ClampSelectedIndex();
            listView.ScrollToIndex(listView.SelectedIndex);
            listView.RefreshShownValue();
            //  eventResultsHeader.UpdateAccordingToSelectedEvent(item);
            //Debug.Log($"SelectJumper index: {listView.SelectedIndex}");
        }

    }
}