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

        private void Start()
        {
            SetupList();
            LoadData();
            initialized = true;
        }

        private void OnEnable()
        {
            if (!initialized) return;
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
            randomEvents = save.randomEvents.OrderByDescending(e => e.competitorId).ToList();
            competitors = save.competitors;
            listView.Items = randomEvents;
            listView.Refresh();
        }

        private void BindListItem(int index, RandomEventListItem listItem)
        {
            var e = randomEvents[index];
            var comp = competitors[e.competitorId].competitor;

            var fullName = $"{comp.firstName} {comp.lastName.ToUpper()}";
            var flag = flagsData.GetFlag(comp.countryCode);

            // Set flag
            listItem.flagImage.sprite = flag;

            // Set icon depending on event type
            switch (e.eventType)
            {
                case RandomEventType.SkillChange:
                    listItem.iconImage.sprite = e.skillChange > 0
                        ? listItem.upArrow
                        : listItem.downArrow;
                    break;

                case RandomEventType.Injury:
                    listItem.iconImage.sprite = listItem.injuryCross;
                    break;
            }

            // Set full event text (localized and including name)
            listItem.eventText.text = e.GetLocalizedDescription($"{comp.firstName} {comp.lastName}");
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
