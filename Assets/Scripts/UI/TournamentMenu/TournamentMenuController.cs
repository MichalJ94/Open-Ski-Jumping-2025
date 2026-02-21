using System;
using OpenSkiJumping.Data;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using EventType = OpenSkiJumping.Competition.EventType;
using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.UI;
using OpenSkiJumping.UI.TournamentMenu;
using OpenSkiJumping.Competition.Persistent;
using TMPro;
using System.Linq;
using System.Collections.Generic;

namespace OpenSkiJumping.UI.TournamentMenu
{
    public class TournamentMenuController : MonoBehaviour, ITournamentMenuController
    {
        [SerializeField] private GameObject nextEventGO;
        [SerializeField] private GameObject classificationsHierarchyGO;
        [SerializeField] private GameObject jumpersListGO;
        [SerializeField] private Button playNextEventButton;
        [SerializeField] private Button startNextSeasonButton;

        [SerializeField] private MainMenuController menuController;
        [SerializeField] private RandomEventsController randomEventsController;
        [SerializeField] private SavesRuntime saves;
        [SerializeField] private GameObject teamsListGO;
        [SerializeField] private GameObject teamSquadGO;
        [SerializeField] private TournamentMenuData tournamentMenuData;

        [SerializeField] private TranslatablePhrase posSkillChangePhrase;
        [SerializeField] private TranslatablePhrase negSkillChangePhrase;


        [SerializeField] private CalendarsRuntime calendarsRuntime;
        [SerializeField] private CompetitorsRuntime competitorsRuntime;
        [SerializeField] private SavesRuntime savesRuntime;

        [SerializeField] private GameObject popUpRoot;
        [SerializeField] private TMP_InputField input;
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button cancelButton;

        public event Action OnReloadTeamsList;

        public void ShowTeamSquad()
        {
            teamSquadGO.SetActive(true);
        }

        public void HideTeamSquad()
        {
            teamSquadGO.SetActive(false);
            OnReloadTeamsList?.Invoke();
        }

        private bool IsSeasonFinished()
        {
            var save = saves.GetCurrentSave();
            return save.resultsContainer.eventIndex >= save.calendar.events.Count;
        }

        private void Awake()
        {
            submitButton.onClick.AddListener(CreateNextSeasonSave);
            cancelButton.onClick.AddListener(() => popUpRoot.SetActive(false));
        }


        private void Start()
        {


            /* // WORKFLOW JONKA!
            if (tournamentMenuData.GetCurrentEvent() == null)
            {
                Debug.Log("Running tournamentMenuData.GetCurrentEvent()");
                nextEventGO.SetActive(false);
                classificationsHierarchyGO.SetActive(false);
                jumpersListGO.SetActive(false);
                teamsListGO.SetActive(false);
                startNextSeasonButton.gameObject.SetActive(true);
                playNextEventButton.interactable = false;
                playNextEventButton.gameObject.SetActive(false);
                return;
            }*/

           if (IsSeasonFinished())
            {
                Debug.Log("Season finished - enabling Start Next Season");

                nextEventGO.SetActive(false);
                classificationsHierarchyGO.SetActive(false);
                jumpersListGO.SetActive(false);
                teamsListGO.SetActive(false);

                playNextEventButton.gameObject.SetActive(false);
                startNextSeasonButton.gameObject.SetActive(true);

                return;
            }

            nextEventGO.SetActive(true);
            classificationsHierarchyGO.SetActive(true);
            jumpersListGO.SetActive(tournamentMenuData.GetCurrentEvent().eventType == EventType.Individual);
            teamsListGO.SetActive(tournamentMenuData.GetCurrentEvent().eventType == EventType.Team);
            randomEventsController.AddRandomEvents();
            startNextSeasonButton.onClick.AddListener(StartNewSeason);
        }

        private void OpenPopUp()
        {
            popUpRoot.SetActive(true);
            input.text = "";
        }

        private void ClosePopUp()
        {
            popUpRoot.SetActive(false);
        }

        private List<Calendar> calendars;

        private void SetupCalendarDropdown()
        {
            calendars = calendarsRuntime.GetData().ToList();

            dropdown.ClearOptions();
            dropdown.AddOptions(calendars.Select(c => c.name).ToList());
        }


        public void StartNewSeason()
        {
            popUpRoot.SetActive(true);

            calendars = calendarsRuntime.GetData().ToList();

            dropdown.ClearOptions();
            dropdown.AddOptions(
                calendars.Select(c => c.name).ToList()
            );

            input.text = "";
        }

        public void LoadCompetition()
        {
            menuController.LoadTournament();
        }

        public void LoadMainMenu()
        {
            menuController.LoadMainMenu();
        }


        private void CreateNextSeasonSave()
        {
            if (string.IsNullOrEmpty(input.text))
                return;

            var oldSave = savesRuntime.GetCurrentSave();
            var selectedCalendar = calendars[dropdown.value];

            // Create normal save first
            GameSave newSave = new GameSave(
                input.text,
                selectedCalendar,
                competitorsRuntime
            );

            // Copy competitors WITH SKILLS
            newSave.competitors = oldSave.competitors
                .Select(c => new CompetitorData
                {
                    calendarId = c.calendarId,
                    competitor = c.competitor,
                    registered = true,
                    teamId = c.teamId
                })
                .ToList();

            // Copy teams
            newSave.teams = oldSave.teams
                .Select(t => new TeamData
                {
                    calendarId = t.calendarId,
                    team = t.team,
                    registered = true,
                    competitors = t.competitors
                        .Select(c => new CompetitorData
                        {
                            calendarId = c.calendarId,
                            competitor = c.competitor,
                            registered = true,
                            teamId = c.teamId
                        })
                        .ToList()
                })
                .ToList();

            savesRuntime.Add(newSave);

            // Set current save
            savesRuntime.Data.currentSaveId =
                savesRuntime.GetData().Count - 1;

            ClosePopUp();

            menuController.LoadTournamentMenu();
        }

        /*
        public void RandomEventSystemTest()
        {
            //Debug.Log("Running RandomEventSystemTest");
            var save = saves.GetCurrentSave();
            int number = UnityEngine.Random.Range(0, save.competitors.Count);
            int skillChange = UnityEngine.Random.Range(-5, 5);

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
                    0

                );
                    save.randomEvents.Add(randomEvent);
                }
                else
                {
                    var randomEvent = new RandomEventData(
                                        number,
                                        skillChange,
                                        negSkillChangePhrase,
                                        0);
                    save.randomEvents.Add(randomEvent);
                }



                Debug.Log("All random events so far:");
                foreach (var item in save.randomEvents)
                {
                    var comp = save.competitors[item.competitorId].competitor;
                    string compName = $"{comp.firstName} {comp.lastName}";
                    Debug.Log(item.GetLocalizedDescription(compName));
                }

                randomEventsController.Show();
            }*/












    }
}
