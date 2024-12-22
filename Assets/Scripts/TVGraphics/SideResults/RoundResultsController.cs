using System.Collections.Generic;
using System.Globalization;
using OpenSkiJumping.Competition.Runtime;
using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.UI.ListView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace OpenSkiJumping.TVGraphics.SideResults
{
    public abstract class RoundResultsController : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] protected FlagsData flagsData;
        [SerializeField] protected RoundResultsListView listView;
        [SerializeField] protected RuntimeResultsManager resultsManager;
        [SerializeField] protected RuntimeCompetitorsList competitorsList;
        [SerializeField] protected List<int> listViewItems;

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
            listItem.rankText.text = $"{item.Rank}";
            listItem.nameText.text = $"{GetNameById(globalId)}";
            listItem.countryFlagText.text = $"{GetCountryCodeById(globalId)}";
            listItem.countryFlagImage.sprite = flagsData.GetFlag(GetCountryCodeById(globalId));
            listItem.resultText.text = $"{item.TotalPoints.ToString("F1", CultureInfo.InvariantCulture)}";
            listItem.distanceText.text = $"{item.Distance.ToString("F1", CultureInfo.InvariantCulture)} m";
            listItem.gateText.text = $"{item.ActualGate}";
            listItem.previousRoundDistanceText.text = $"{item.PreviousRoundDistance.ToString("F1", CultureInfo.InvariantCulture)} m";
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

            if (item.Style > 0)
            {
                listItem.styleText.text = $"{item.Style.ToString("F1", CultureInfo.InvariantCulture)}";
            }
            else
            {
                listItem.styleText.text = "";
            }

            if (item.PreviousRoundStyle > 0 && item.PreviousRoundStyle <= 60)
            {
                listItem.previousRoundStyleText.text = $"{item.PreviousRoundStyle.ToString("F1", CultureInfo.InvariantCulture)}";
            }
            else
            {
                listItem.previousRoundStyleText.text = "";
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