using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.UI.JumpersMenu;
using OpenSkiJumping.UI.ListView;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        [SerializeField] private JumpersSelectionListView listView;

        public CompetitorData SelectedJumper { get; private set; }

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

        public event Action OnDataReload;
        public event Action<CompetitorData> OnJumperSelected;
        public event Action OnSelectionChanged;

        private void Start()
        {
            ListViewSetup();
            presenter = new IndResJumpersSelectionPresenter(this, tournamentMenuData);
            initialized = true;
        }

        private void OnEnable()
        {
            if (!initialized) return;
            OnDataReload?.Invoke();
            listView.Reset();
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
        private void HandleJumperSelected(CompetitorData jumper)
        {
            SelectedJumper = jumper;
            OnJumperSelected?.Invoke(jumper);
        }

    }
}