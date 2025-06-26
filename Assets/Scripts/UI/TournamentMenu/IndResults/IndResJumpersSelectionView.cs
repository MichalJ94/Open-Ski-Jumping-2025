using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.UI.JumpersMenu;
using OpenSkiJumping.UI.ListView;
using OpenSkiJumping.UI.TournamentMenu.ResultsMenu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

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
      
        public event Action OnDataReload;
        public event Action<CompetitorData> OnJumperSelected;
        public event Action OnSelectionChanged;

        private void Start()
        {
            ListViewSetup();
            indResultsListController.Initialize();
            presenter = new IndResJumpersSelectionPresenter(this, tournamentMenuData);
            initialized = true;
            
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
            uiItem.rankText.text = data.rank.ToString();
            uiItem.countryCodeText.text = data.countryCode;
            uiItem.valueText.text = data.value.ToString("F1");
            uiItem.countryFlagImage.sprite = flagsData.GetFlag(data.countryCode);
        }

        private void ListViewSetup()
        {
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
        private void HandleJumperSelected(int index)
        {
            if (index < 0 || index >= jumpers.Count) return;

            var jumper = jumpers[index];
            SelectedJumper = jumper;

            OnJumperSelected?.Invoke(jumper); // Let presenter or controller listen to this

            // Example logic to fetch results:
            var jumperResults = GetResultsForJumper(jumper);
            indResultsListController.Results = jumperResults;
        }

        private List<IndResultsListItem> GetResultsForJumper(CompetitorData jumper)
        {
            var jumperResults = new List<IndResultsListItem>();

            for (int i = 0; i < tournamentMenuData.GameSave.resultsContainer.eventResults.Length; i++)
            {
                var result = tournamentMenuData.GameSave.resultsContainer.eventResults[i];
                int jumperIndex = result.competitorIds.IndexOf(jumper.calendarId);

                if (jumperIndex >= 0)
                {
                    var compResult = result.results[jumperIndex];
                    jumperResults.Add(new IndResultsListItem
                    {
                        name = jumper.competitor.lastName,
                        countryCode = jumper.competitor.countryCode,
                        rank = compResult.Rank,
                        value = compResult.TotalPoints // or whatever stat you want
                    });
                }
            }

            return jumperResults;
        }

        private void SelectJumper(CompetitorData item)
        {
            listView.SelectedIndex =
                item == null ? listView.SelectedIndex : jumpers.IndexOf(item);

            listView.ClampSelectedIndex();
            listView.ScrollToIndex(listView.SelectedIndex);
            listView.RefreshShownValue();
            //  eventResultsHeader.UpdateAccordingToSelectedEvent(item);
            Debug.Log($"SelectJumper index: {listView.SelectedIndex}");
        }

    }
}