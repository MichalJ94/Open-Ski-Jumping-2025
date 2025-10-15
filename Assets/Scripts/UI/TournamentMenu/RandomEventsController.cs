using System.Collections.Generic;
using System.Linq;
using OpenSkiJumping.ScriptableObjects;
using UnityEngine;
using OpenSkiJumping.UI.ListView;
using OpenSkiJumping.Data;
using OpenSkiJumping.UI.TournamentMenu.ResultsMenu;

namespace OpenSkiJumping.UI.TournamentMenu
{
    public class RandomEventsController : MonoBehaviour
    {
        [SerializeField] private TournamentMenuData tournamentMenuData;
        [SerializeField] private FlagsData flagsData;
        [SerializeField] private RandomEventsListView listView;
        [SerializeField] private EventsSelectionView eventsSelectionView;
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private GameObject maskObject;
        [SerializeField] private SavesRuntime saves;

        [SerializeField] private TranslatablePhrase posSkillChangePhrase;
        [SerializeField] private TranslatablePhrase negSkillChangePhrase;

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

            randomEvents = save.randomEvents.OrderBy(e => e.eventIndex).ToList();
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
            //listItem.eventText.text = e.GetLocalizedDescription(fullName);

            string eventInfo = "";
            if (e.eventIndex > 0 && e.eventIndex <= tournamentMenuData.Calendar.events.Count)
            {
                var eventData = tournamentMenuData.Calendar.events[e.eventIndex - 1];
                eventInfo = $"  ({e.eventIndex} {eventData.hillId})";
            }
            else
            {
                eventInfo = "unknown event";
            }

            listItem.eventText.text = e.GetLocalizedDescription(fullName, eventInfo);

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


        public void AddRandomEvents()
        {
            //Debug.Log("Running RandomEventSystemTest");
            var save = saves.GetCurrentSave();
            int number = UnityEngine.Random.Range(0, save.competitors.Count);
            int skillChange = UnityEngine.Random.Range(-5, 5);
            if(save.resultsContainer.eventIndex != 0) {
                if (skillChange != 0)
                {
                    var competitor = save.competitors[number].competitor;
                    competitor.normalHillSkill += skillChange;
                    competitor.largeHillSkill += skillChange;
                    competitor.skiFlyingHillSkill += skillChange;
                    Debug.Log($"{competitor.firstName} {competitor.lastName} got the skill change {skillChange}");

                    if (skillChange > 0)
                    {
                        var randomEvent = new RandomEventData(
                        number,
                        skillChange,
                        posSkillChangePhrase,
                        0,
                        save.resultsContainer.eventIndex

                    );
                        save.randomEvents.Add(randomEvent);
                    }
                    else
                    {
                        var randomEvent = new RandomEventData(
                                            number,
                                            skillChange,
                                            negSkillChangePhrase,
                                            0,
                                            save.resultsContainer.eventIndex);
                        save.randomEvents.Add(randomEvent);
                    }
                }
                Debug.Log($"Events so far: {save.resultsContainer.eventIndex} Latest one: {save.resultsContainer.eventIndex} {save.calendar.events[save.resultsContainer.eventIndex-1].hillId}");

                Debug.Log("All random events so far:");
                foreach (var item in save.randomEvents)
                {
                    var comp = save.competitors[item.competitorId].competitor;
                    string compName = $"{comp.firstName} {comp.lastName}";
                    Debug.Log(item.GetLocalizedDescription(compName));
                }


                Show();
            }


        }
    }
}
