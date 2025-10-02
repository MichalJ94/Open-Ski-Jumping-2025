using System;
using OpenSkiJumping.Data;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using EventType = OpenSkiJumping.Competition.EventType;

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
            Debug.Log($"From RandomEventSystemTest save.competitors.Count: {save.competitors.Count}");


            int number = Random.Range(0, save.competitors.Count);
            int skillChange = Random.Range(-5,5);
            if(skillChange != 0)
            {
                save.competitors[number].competitor.normalHillSkill += skillChange;
                save.competitors[number].competitor.largeHillSkill += skillChange;
                save.competitors[number].competitor.skiFlyingHillSkill += skillChange;
                save.randomEvents.Add($"{save.competitors[number].competitor.id} skills {skillChange}");
                Debug.Log("All random events so far:");
                foreach(string item in save.randomEvents)
                {
                    Debug.Log(item);
                }

            
            }
            




        }



    }
}