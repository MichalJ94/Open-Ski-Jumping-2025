using System.Collections.Generic;
using System.Globalization;
using OpenSkiJumping.Competition.Runtime;
using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.UI.ListView;
using UnityEngine;

namespace OpenSkiJumping.TVGraphics.SideResults
{
    public abstract class RoundResultsController : MonoBehaviour
    {
        [SerializeField] protected FlagsData flagsData;
        [SerializeField] protected RoundResultsListView listView;
        [SerializeField] protected RuntimeResultsManager resultsManager;
        [SerializeField] protected RuntimeCompetitorsList competitorsList;
        [SerializeField] protected List<int> listViewItems;


        public void Start()
        {
            listViewItems = new List<int>();
            listView.SelectionType = SelectionType.None;
            listView.Items = listViewItems;
            listView.Initialize(BindListViewItem);
        }

        private void BindListViewItem(int index, RoundResultsListItem listItem)
        {
            var localId = resultsManager.Value.GetIdByRank(index);
            var globalId = resultsManager.Value.OrderedParticipants[localId].id;
            var item = resultsManager.Value.Results[localId];
            var roundNumber = resultsManager.Value.GetRoundNumber();
            listItem.rankText.text = $"{item.Rank}";
            listItem.nameText.text = $"{GetNameById(globalId)}";
            listItem.countryFlagText.text = $"{GetCountryCodeById(globalId)}";
            listItem.countryFlagImage.sprite = flagsData.GetFlag(GetCountryCodeById(globalId));
            listItem.resultText.text = $"{item.TotalPoints.ToString("F1", CultureInfo.InvariantCulture)}";
            listItem.distanceText.text = $"{item.Distance.ToString("F1", CultureInfo.InvariantCulture)} m";
            listItem.previousRoundDistanceText.text = $"{item.PreviousRoundDistance.ToString("F1", CultureInfo.InvariantCulture)} m";
            if(roundNumber == 0)
            {
                listItem.previousRoundDistanceText.enabled = false;
            }
            else
            {
                listItem.previousRoundDistanceText.enabled = true;
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
}