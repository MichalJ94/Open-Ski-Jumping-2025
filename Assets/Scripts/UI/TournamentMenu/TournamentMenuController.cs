using System;
using OpenSkiJumping.Data;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using EventType = OpenSkiJumping.Competition.EventType;
using OpenSkiJumping.ScriptableObjects;

namespace OpenSkiJumping.UI.TournamentMenu
{
    public class TournamentMenuController : MonoBehaviour, ITournamentMenuController
    {
        [SerializeField] private GameObject nextEventGO;
        [SerializeField] private GameObject classificationsHierarchyGO;
        [SerializeField] private GameObject jumpersListGO;
        [SerializeField] private Button playNextEventButton;

        [SerializeField] private MainMenuController menuController;
        [SerializeField] private SavesRuntime saves;
        [SerializeField] private GameObject teamsListGO;
        [SerializeField] private GameObject teamSquadGO;
        [SerializeField] private TournamentMenuData tournamentMenuData;

        [SerializeField] private TranslatablePhrase skillChangePhrase;

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


                var randomEvent = new RandomEventData(
                    number,
                    skillChange,
                    skillChangePhrase
                );

                save.randomEvents.Add(randomEvent);

                Debug.Log("All random events so far:");
                foreach (var item in save.randomEvents)
                {
                    Debug.Log(item.GetLocalizedDescription(save.competitors[item.competitorId].competitor.id));
                }
            }
        }











    }
}
