using System.Collections.Generic;
using System.Linq;
using OpenSkiJumping.Competition;

namespace OpenSkiJumping.UI.TournamentMenu.ResultsMenu
{
    public class EventsSelectionPresenter
    {
        private readonly TournamentMenuData model;
        private readonly IEventsSelectionView view;

        public EventsSelectionPresenter(IEventsSelectionView view, TournamentMenuData model)
        {
            this.model = model;
            this.view = view;

            InitEvents();
            SetInitValues();
        }

        private void PresentList()
        {
            view.Events = model.GameSave.calendar.events.Take(model.GameSave.resultsContainer.eventIndex);
        }

        private void SetResults()
        {
            var index = view.CurrentEventIndex;
            UnityEngine.Debug.Log("Od EventsSelectionPresenter index: " + index);
            if (index == -1) return;
            var competitors = (view.SelectedEvent.eventType == EventType.Individual
                ? model.GameSave.competitors.Select(it =>
                    (name: $"{it.competitor.firstName} {it.competitor.lastName.ToUpper()}", it.competitor.countryCode))
                : model.GameSave.teams.Select(it => (name: it.team.teamName, it.team.countryCode))).ToList();
            var eventResults = model.GameSave.resultsContainer.eventResults[index];
            var tmp = eventResults.allroundResults.Select(it => new ResultsListItem
            {
                rank = eventResults.results[it].Rank,
                name = competitors[eventResults.competitorIds[it]].name,
                countryCode = competitors[eventResults.competitorIds[it]]
                    .countryCode,
                value = eventResults.results[it].TotalPoints,
                distance = eventResults.results[it].Results[0].results[eventResults.results[it].Results[0].results.Count - 1].distance,
                style = eventResults.results[it].Results[0].results[eventResults.results[it].Results[0].results.Count - 1].judgesTotalPoints,
                previousRoundStyle = eventResults.results[it].PreviousRoundStyle,
                //distance = eventResults.results[it].Distance,
                // distance = eventResults.results[it].Results[0].results[eventResults.results[it].Results[index].results.Count-1].distance,



                previousRoundDistance = eventResults.results[it].PreviousRoundDistance,












            }).ToList();
            view.ResultsListController.Results = tmp;
            
        }


        private void InitEvents()
        {
            view.OnSelectionChanged += SetResults;
            view.OnDataReload += SetInitValues;
        }

        private void SetInitValues()
        {
            PresentList();
            view.SelectedEvent = model.GameSave.calendar.events.FirstOrDefault();
            SetResults();
        }
    }
}