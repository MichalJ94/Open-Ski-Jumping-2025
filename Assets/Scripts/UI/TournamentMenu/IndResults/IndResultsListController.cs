using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.UI.ListView;
using UnityEngine;
namespace OpenSkiJumping.UI.TournamentMenu.ResultsMenu
{
    public enum ResultBackgroundStyle
    {
        NoResult,
        Top30,
        Bottom20,
        Qualification,
        Trial
    }

    public class IndResultsListItem
    {
        public string competitionID;
        public string hillName;
        public string name;
        public string countryCode;
        public int rank;
        public decimal value;
        public ResultBackgroundStyle backgroundStyle;
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

            // Show "HillName (#ID)"
            uiItem.nameText.text = $" ({data.competitionID}) {data.hillName}";

            // Show rank or "-"
            uiItem.rankText.text = data.rank > 0 ? data.rank.ToString() : "-";

            // Optionally show other data
            // uiItem.valueText.text = data.rank > 0 ? data.value.ToString("F1") : "-";
            // uiItem.countryCodeText.text = data.countryCode;
            // uiItem.countryFlagImage.sprite = flagsData.GetFlag(data.countryCode);
        }
    }
}
