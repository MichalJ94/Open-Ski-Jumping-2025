using System;
using OpenSkiJumping.Data;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using EventType = OpenSkiJumping.Competition.EventType;
using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.UI;
using OpenSkiJumping.UI.TournamentMenu;

namespace OpenSkiJumping.UI.TournamentMenu
{
    public class TournamentMenuController : MonoBehaviour, ITournamentMenuController
    {
        [SerializeField] private GameObject nextEventGO;
        [SerializeField] private GameObject classificationsHierarchyGO;
        [SerializeField] private GameObject jumpersListGO;
        [SerializeField] private Button playNextEventButton;

        [SerializeField] private MainMenuController menuController;
        [SerializeField] private RandomEventsController randomEventsController;
        [SerializeField] private SavesRuntime saves;
        [SerializeField] private GameObject teamsListGO;
        [SerializeField] private GameObject teamSquadGO;
        [SerializeField] private TournamentMenuData tournamentMenuData;

        [SerializeField] private TranslatablePhrase posSkillChangePhrase;
        [SerializeField] private TranslatablePhrase negSkillChangePhrase;

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

        private void Start()
        {
            if (tournamentMenuData.GetCurrentEvent() == null)
            {
                Debug.Log("Running tournamentMenuData.GetCurrentEvent()");
                nextEventGO.SetActive(false);
                classificationsHierarchyGO.SetActive(false);
                jumpersListGO.SetActive(false);
                teamsListGO.SetActive(false);
                playNextEventButton.interactable = false;
                return;
            }

            classificationsHierarchyGO.SetActive(true);
            jumpersListGO.SetActive(tournamentMenuData.GetCurrentEvent().eventType == EventType.Individual);
            teamsListGO.SetActive(tournamentMenuData.GetCurrentEvent().eventType == EventType.Team);
            RandomEventSystemTest();
        }

        public void LoadCompetition()
        {
            menuController.LoadTournament();
        }

        public void LoadMainMenu()
        {
            menuController.LoadMainMenu();
        }

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
            }
        }











    }
}
