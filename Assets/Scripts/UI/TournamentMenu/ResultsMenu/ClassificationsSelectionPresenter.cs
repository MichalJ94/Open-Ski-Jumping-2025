using System;
using System.Diagnostics;
using System.Linq;
using OpenSkiJumping.Competition;
using OpenSkiJumping.Data;

namespace OpenSkiJumping.UI.TournamentMenu.ResultsMenu
{
    public class ClassificationsSelectionPresenter
    {
        private readonly TournamentMenuData model;
        private readonly IClassificationsSelectionView view;
        private readonly SavesRuntime savesRuntime;

        public ClassificationsSelectionPresenter(IClassificationsSelectionView view, TournamentMenuData model)
        {
            this.model = model;
            this.view = view;

            InitEvents();
            SetInitValues();
        }

        private void PresentList()
        {
            view.Classifications = model.GameSave.calendar.classifications;
        }

        private void SetResults()
        {
            var index = view.CurrentClassificationIndex;
            var competitors = (view.SelectedClassification.eventType == EventType.Individual
                ? model.GameSave.competitors.Select(it =>
                    (name: $"{it.competitor.firstName} {it.competitor.lastName.ToUpper()}", it.competitor.countryCode))
                : model.GameSave.teams.Select(it => (name: it.team.teamName, it.team.countryCode))).ToList();


            view.ResultsListController.Results = model.GameSave.resultsContainer.classificationResults[index]
                .totalSortedResults.Select(it => new ResultsListItem
                {
                    rank = model.GameSave.resultsContainer.classificationResults[index].rank[it],
                    name = competitors[it].name,
                    countryCode = competitors[it].countryCode,
                    value = model.GameSave.resultsContainer.classificationResults[index].totalResults[it],
                    style = CountFirstPlaces(index, it)
                }); ;
          //  UnityEngine.Debug.Log("Od ClassificationPresenter index: " + index);
        }


        private void InitEvents()
        {
            view.OnSelectionChanged += SetResults;
            view.OnDataReload += SetInitValues;
        }

        private void SetInitValues()
        {
            PresentList();
            view.SelectedClassification = model.GameSave.calendar.classifications.FirstOrDefault();
           // CountFirstPlaces();
            SetResults();
        }

        private int CountFirstPlaces(int classificationIndex, int jumper)
        {
            var index = view.CurrentClassificationIndex;
            var eventCount = model.GameSave.resultsContainer.eventIndex;
            UnityEngine.Debug.Log("Od CountFirstPLaces eventCount:" + eventCount);
            var eventResults = model.GameSave.resultsContainer.eventResults;
            return eventCount;
            /*for (int i = 0, i < save.)
                return 1;*/
        }
    }
}