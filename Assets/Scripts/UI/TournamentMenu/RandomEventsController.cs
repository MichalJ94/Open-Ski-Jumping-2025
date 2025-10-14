using System.Collections.Generic;
using System.Linq;
using OpenSkiJumping.ScriptableObjects;
using UnityEngine;
using OpenSkiJumping.UI.ListView;

namespace OpenSkiJumping.UI.TournamentMenu
{
    public class RandomEventsController : MonoBehaviour
    {
        [SerializeField] private TournamentMenuData tournamentMenuData;
        [SerializeField] private FlagsData flagsData;
        [SerializeField] private RandomEventsListView listView;
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private GameObject maskObject;

        private List<RandomEventData> randomEvents;
        private List<CompetitorData> competitors;

        private bool initialized;

        private void Awake()
        {
            SetupList();
        }

        private void Start()
        {
            LoadData();
            initialized = true;
        }

        private void OnEnable()
        {
            if (initialized)
                LoadData();
        }

        private void SetupList()
        {
            listView.SelectionType = SelectionType.None;
            listView.Initialize(BindListItem);
        }

        private void LoadData()
        {
            var save = tournamentMenuData.GameSave;

            if (save == null)
            {
                Debug.LogError("GameSave is null!");
                return;
            }

            if (save.randomEvents == null)
            {
                Debug.LogError("save.randomEvents is null!");
                return;
            }

            if (save.competitors == null)
            {
                Debug.LogError("save.competitors is null!");
                return;
            }

            randomEvents = save.randomEvents.OrderByDescending(e => e.competitorId).ToList();
            competitors = save.competitors;

            Debug.Log($"Loaded {randomEvents.Count} random events and {competitors.Count} competitors.");

            listView.Items = randomEvents;
            listView.Refresh();
        }

        private void BindListItem(int index, RandomEventListItem listItem)
        { 


            var e = randomEvents[index];
            var comp = competitors[e.competitorId].competitor;

            if (e == null)
            {
                Debug.LogError($"Null RandomEventData at index {index}");
                return;
            }

            if (competitors == null)
            {
                Debug.LogError("Competitors list is null!");
                return;
            }

            if (e.competitorId < 0 || e.competitorId >= competitors.Count)
            {
                Debug.LogError($"Invalid competitorId {e.competitorId} for random event at index {index}. Competitors count: {competitors.Count}");
                return;
            }

            if (competitors[e.competitorId].competitor == null)
            {
                Debug.LogError($"Competitor object is null at ID {e.competitorId}");
                return;
            }

            // Full name and flag
            var fullName = $"{comp.firstName} {comp.lastName.ToUpper()}";
            var flag = flagsData.GetFlag(comp.countryCode);

            // Set flag
            listItem.flagImage.sprite = flag;

            // Icon: use upArrow if skillChange > 0, downArrow if skillChange < 0, and maybe hide if 0
            if (e.skillChange > 0)
            {
                listItem.iconImage.sprite = listItem.upArrow;
                listItem.iconImage.enabled = true;
            }
            else if (e.skillChange < 0)
            {
                listItem.iconImage.sprite = listItem.downArrow;
                listItem.iconImage.enabled = true;
            }
            else
            {
                // Neutral: hide the icon or show a neutral sprite if you want
                listItem.iconImage.enabled = false;
            }

            // Set event text (localized description)
            listItem.eventText.text = e.GetLocalizedDescription(fullName);
        }


        public void Show()
        {
            popupPanel.SetActive(true);
            //maskObject.SetActive(true);
            LoadData();
        }

        public void Hide()
        {
            popupPanel.SetActive(false);
            //maskObject.SetActive(false);
        }
    }
}
