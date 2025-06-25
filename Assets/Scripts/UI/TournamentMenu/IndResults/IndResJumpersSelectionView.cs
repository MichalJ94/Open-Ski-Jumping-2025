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

            listView.SelectionType = SelectionType.None;
            listView.Initialize(BindListViewItem);
        }

        private void BindListViewItem(int index, JumpersListItem listItem)
        {
            var item = jumpers[index];

            listItem.nameText.text = $"{item.competitor.firstName} {item.competitor.lastName.ToUpper()}";
            listItem.countryFlagText.text = item.competitor.countryCode;
            listItem.countryFlagImage.sprite = flagsData.GetFlag(item.competitor.countryCode);
            listItem.genderIconImage.sprite = iconsData.GetGenderIcon(item.competitor.gender);

            listItem.toggleExtension.SetElementId(index);
            listItem.toggleExtension.Toggle.SetIsOnWithoutNotify(item.registered);
        }

        private void HandleSelectionChanged(int index, bool value)
        {
            var item = jumpers[index];
        }
    }
}