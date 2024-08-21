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
            var eventResults = model.GameSave.resultsContainer.eventResults;
            int competitorIDsofWinner;
            int actualIDofWinner;
            int firstPlacesCount = 0;


            for (int i = 0; i < model.GameSave.calendar.events.Count; i++)
            {
                for (int j = 0; j < model.GameSave.calendar.events[i].classifications.Count; j++)
                {
                    if (j == classificationIndex)
                    {
                        competitorIDsofWinner = eventResults[i].finalResults[0];
                        actualIDofWinner = eventResults[i].competitorIds[competitorIDsofWinner];
                        UnityEngine.Debug.Log("firstPlacesCounter eventResults[i] " + i + " classificationIndex: " + j + " competitorIDsofWinner: " + competitorIDsofWinner + " actualIDofWinner " + actualIDofWinner);
                    }



                    /*for (int k = 0; k < eventResults[i].competitorIds.Count; k++)
                    {
                        if (eventResults[i].finalResults[0] == 0)
                        {
                            UnityEngine.Debug.Log("Dla konkursu eventResults[i] gdzie i to: " + i + "finalResults rowne 0 dla competitorIds" + eventResults[i].competitorIds[k]);
                            /*  if (eventResults[i].competitorIds[k] == jumper)
                              {
                                  UnityEngine.Debug.Log("Zwyciezca konkursu o id " + i + " jest zawodnik o ID" + k);
                              }

                        }                        
                    }
                    // UnityEngine.Debug.Log("firstPlacesCounter do tej klasyfikacji bierzemy info z konkursu numer o id" + i + " a id klasyfikacji to: " + j);
                    /*if (model.GameSave.resultsContainer.classificationResults[classificationIndex].rank[jumper] == 1)
                    {
                        UnityEngine.Debug.Log("Zwyciêzc¹ konkursu ");
                    }*/
                }
            }


            /*for (int i = 0; i < eventCount; i++)
            {
                if (eventResults[i].allroundResults[jumper] == 0 && eventResults[i].competitorIds[jumper] == jumper)
                {
                    UnityEngine.Debug.Log("firstPlacesCounter increased. I: " + i + " jumper: " + jumper + " classificationIndex: " + classificationIndex);
                    firstPlacesCount ++;
                }
                return firstPlacesCount;
            }*/



            return eventCount;
        }
    }
}
