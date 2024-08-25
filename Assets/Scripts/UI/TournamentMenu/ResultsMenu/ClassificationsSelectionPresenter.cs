using System;
using System.Collections.Generic;
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
        int firstPlacesCount, secondPlacesCount, thirdPlacesCount;
        List<(string,string)> competitorsFetch;
        public ClassificationsSelectionPresenter(IClassificationsSelectionView view, TournamentMenuData model)
        {
            this.model = model;
            this.view = view;

            InitEvents();
            SetInitValues();
            CheckForMultipleJumpersWhoTookSamePodiumPlace();
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
            competitorsFetch = competitors;

            view.ResultsListController.Results = model.GameSave.resultsContainer.classificationResults[index]
                .totalSortedResults.Select(it => new ResultsListItem
                {
                    rank = model.GameSave.resultsContainer.classificationResults[index].rank[it],
                    name = competitors[it].name,
                    countryCode = competitors[it].countryCode,
                    value = model.GameSave.resultsContainer.classificationResults[index].totalResults[it],
                    style = CountFirstPlaces(index, it),
                    previousRoundStyle = CountSecondPlaces(index, it),
                    previousRoundDistance = CountThirdPlaces(index, it)
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
            firstPlacesCount = 0;

            for (int i = 0; i < eventCount; i++)
            {
                for (int j = 0; j < model.GameSave.calendar.events[i].classifications.Count; j++)
                {
                    //Ogarnac dlaczego zlicza podia do Ski Flying Cup a nie do TCS
                    if (model.GameSave.calendar.events[i].classifications[j] == classificationIndex && model.GameSave.calendar.classifications[classificationIndex].eventType != EventType.Team && model.GameSave.calendar.events[i].eventType != EventType.Team )
                    {
                        competitorIDsofWinner = eventResults[i].finalResults[0];
                        actualIDofWinner = eventResults[i].competitorIds[competitorIDsofWinner];
                        //UnityEngine.Debug.Log("firstPlacesCounter eventResults[i] " + i + " classificationIndex: " + j + " competitorIDsofWinner: " + competitorIDsofWinner + " actualIDofWinner " + actualIDofWinner);

                        if (actualIDofWinner == jumper)
                        {
                            //UnityEngine.Debug.Log("firstPlacesCounter FIRST PLACE COUNTING NOW!! eventResults[i] " + i + " classificationIndex: " + classificationIndex + " j " + j + " classifications count for this event: " + model.GameSave.calendar.events[i].classifications.Count);
                            firstPlacesCount++;
                        }


                    }

                }
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


            /*for (int i = 0; i < eventCount; i++)
            {
                if (eventResults[i].allroundResults[jumper] == 0 && eventResults[i].competitorIds[jumper] == jumper)
                {
                    UnityEngine.Debug.Log("firstPlacesCounter increased. I: " + i + " jumper: " + jumper + " classificationIndex: " + classificationIndex);
                    firstPlacesCount ++;
                }
                return firstPlacesCount;
            }*/



            return firstPlacesCount;
        }



        private int CountSecondPlaces(int classificationIndex, int jumper)
        {
            var index = view.CurrentClassificationIndex;
            var eventCount = model.GameSave.resultsContainer.eventIndex;
            var eventResults = model.GameSave.resultsContainer.eventResults;
            int competitorIDsof2ndPlace;
            int actualIDof2ndPlace;
            secondPlacesCount = 0;


            for (int i = 0; i < eventCount; i++)
            {
                for (int j = 0; j < model.GameSave.calendar.events[i].classifications.Count; j++)
                {
                    if (model.GameSave.calendar.events[i].classifications[j] == classificationIndex && model.GameSave.calendar.events[i].eventType != EventType.Team && model.GameSave.calendar.classifications[j].eventType != EventType.Team )
                    {
                        competitorIDsof2ndPlace = eventResults[i].finalResults[1];
                        actualIDof2ndPlace = eventResults[i].competitorIds[competitorIDsof2ndPlace];
                       // UnityEngine.Debug.Log("secondPlacesCounter eventResults[i] " + i + " classificationIndex: " + j + " competitorIDsofWinner: " + competitorIDsof2ndPlace + " actualIDofWinner " + actualIDof2ndPlace);

                        if (actualIDof2ndPlace == jumper)
                        {

                            secondPlacesCount++;
                            UnityEngine.Debug.Log("OD CountSecondPlace just added second place! eventID i:" + i + " competitorsFetch name " + competitorsFetch[jumper].Item1 + " actualIDof2ndPlace " + actualIDof2ndPlace + " eventResults[i].results[jumper].Rank:  " + eventResults[i].results[jumper].Rank);
                            if (eventResults[i].results[jumper].Rank != 2)
                            {
                               //UnityEngine.Debug.Log("OD CountSecondPlace WIDZI ZE RANK != 2!!! Aktualny firstPlaceCount = " + firstPlacesCount + " competitorsFetch name: " + competitorsFetch[jumper].Item1 + " eventID i: " +i);
                            }

                        }


                    }

                }
            }
            return secondPlacesCount;
        }

        private int CountThirdPlaces(int classificationIndex, int jumper)
        {
            var index = view.CurrentClassificationIndex;
            var eventCount = model.GameSave.resultsContainer.eventIndex;
            var eventResults = model.GameSave.resultsContainer.eventResults;
            int competitorIDsof3rdPlace;
            int actualIDof3rdPlace;
            thirdPlacesCount = 0;


            for (int i = 0; i < eventCount; i++)
            {
                for (int j = 0; j < model.GameSave.calendar.events[i].classifications.Count; j++)
                {
                    if (model.GameSave.calendar.events[i].classifications[j] == classificationIndex && model.GameSave.calendar.events[i].eventType != EventType.Team && model.GameSave.calendar.classifications[j].eventType != EventType.Team)
                    {
                        competitorIDsof3rdPlace = eventResults[i].finalResults[2];
                        actualIDof3rdPlace = eventResults[i].competitorIds[competitorIDsof3rdPlace];
                        //UnityEngine.Debug.Log("thirdPlacesCounter eventResults[i] " + i + " classificationIndex: " + j + " competitorIDsofWinner: " + competitorIDsof3rdPlace + " actualIDofWinner " + actualIDof3rdPlace);

                        if (actualIDof3rdPlace == jumper)
                        {
                            thirdPlacesCount++;
                        }


                    }

                }
            }
            return thirdPlacesCount;
        }


        private int CheckForMultipleJumpersWhoTookSamePodiumPlace()
        {
            var eventResults = model.GameSave.resultsContainer.eventResults;
            var eventCount = model.GameSave.resultsContainer.eventIndex;
            var list = new List<Tuple<int, decimal, decimal, decimal, decimal>>();
            //var podiumPlaces = model.GameSave.resultsContainer.eventResults.Select(it => (it.results.));
            for (int i = 0; i < eventCount; i++)
            {
                var allTotalPointsForThisCompetition = new List<KeyValuePair<int, decimal>>();
                var dupaGowno = new List<decimal>();
                for (int j = 0; j < eventResults[i].results.Count; j++)
                {
                    allTotalPointsForThisCompetition.Add(new KeyValuePair<int, decimal>(j, eventResults[i].results[j].TotalPoints));
                    dupaGowno.Add(eventResults[i].results[j].TotalPoints);
                }
                list.Add(new Tuple<int, decimal, decimal, decimal, decimal>(i, dupaGowno.OrderByDescending(r => r).Take(1).LastOrDefault(), dupaGowno.OrderByDescending(r => r).Take(2).LastOrDefault(), dupaGowno.OrderByDescending(r => r).Take(3).LastOrDefault(), dupaGowno.OrderByDescending(r => r).Take(4).LastOrDefault()));
            }
            
            foreach (Tuple<int, decimal, decimal, decimal, decimal> item in list)
            {
                UnityEngine.Debug.Log($"Event index: {item.Item1} highest value: {item.Item2} 2nd highest value {item.Item3}");
            }
                return 0;

        }


    }
}
