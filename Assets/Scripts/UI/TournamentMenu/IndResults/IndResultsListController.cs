using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.UI.ListView;
using UnityEngine;
namespace OpenSkiJumping.UI.TournamentMenu.ResultsMenu
{
    [Serializable]
    public class IndResultsListItem
    {
        public string countryCode;
        public string name;
        public int rank;
        public decimal value;
    }

    public class IndResultsListController : MonoBehaviour
    {
        [SerializeField] private IndResultsListView listView;
        [SerializeField] private FlagsData flagsData;

        private List<IndResultsListItem> results;

        public IEnumerable<IndResultsListItem> Results
        {
            get => results;
            set
            {
                results = value.ToList();
                listView.Items = results;
                listView.Refresh();
            }
        }

        public void Initialize()
        {
            listView.SelectionType = SelectionType.None;
            listView.Initialize(BindListItem);
            Debug.Log("IndResultsListController initialized");
        }

        private void BindListItem(int index, IndResultsListItemUI uiItem)
        {
            var data = results[index];
            uiItem.nameText.text = data.name;
            uiItem.rankText.text = data.rank.ToString();
            uiItem.valueText.text = data.value.ToString("F1");
            uiItem.countryCodeText.text = data.countryCode;
            uiItem.countryFlagImage.sprite = flagsData.GetFlag(data.countryCode);
        }
    }
}
