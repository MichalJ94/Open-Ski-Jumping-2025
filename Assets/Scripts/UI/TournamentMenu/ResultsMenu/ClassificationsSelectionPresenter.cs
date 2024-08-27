using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using OpenSkiJumping.Competition;
using OpenSkiJumping.Data;
using static UnityEditor.Progress;

namespace OpenSkiJumping.UI.TournamentMenu.ResultsMenu
{
    public class ClassificationsSelectionPresenter
    {
        private readonly TournamentMenuData model;
        private readonly IClassificationsSelectionView view;
        private readonly SavesRuntime savesRuntime;
        private readonly ResultsManager resultsManager;
        int firstPlacesCount, secondPlacesCount, thirdPlacesCount;
        List<(string, string)> competitorsFetch;
        public List<UpdatePodiumsItem> samePodiumPlacesList;
        public ClassificationsSelectionPresenter(IClassificationsSelectionView view, TournamentMenuData model)
        {
            this.model = model;
            this.view = view;

            InitEvents();
            samePodiumPlacesList = CheckForMultipleJumpersWhoTookSamePodiumPlace();
            SetInitValues();

        }

        public class UpdatePodiumsItem
        {
            public int EventIndex { get; set; }
            public int PositionInQuestion { get; set; }
            public int ClassificationIndex { get; set; }
            public string JumperName { get; set; }

            public UpdatePodiumsItem(int eventIndex, int positionInQuestion, int classificationIndex, string jumperName)
            {
                EventIndex = eventIndex;
                PositionInQuestion = positionInQuestion;
                ClassificationIndex = classificationIndex;
                JumperName = jumperName;
            }

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


            //TO MOZE TUTAJ ZROBIC CHECK POZYCJI EX EQUEO??

        }


        private void InitEvents()
        {
            view.OnSelectionChanged += SetResults;
            view.OnSelectionChanged += UpdateResults;
            view.OnDataReload += SetInitValues;
        }

        private void SetInitValues()
        {
            PresentList();
            view.SelectedClassification = model.GameSave.calendar.classifications.FirstOrDefault();
            // CountFirstPlaces();
            SetResults();
            UpdateResults();
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

                    if (model.GameSave.calendar.events[i].classifications[j] == classificationIndex && model.GameSave.calendar.classifications[classificationIndex].eventType != EventType.Team && model.GameSave.calendar.events[i].eventType != EventType.Team)
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
                    if (model.GameSave.calendar.events[i].classifications[j] == classificationIndex && model.GameSave.calendar.events[i].eventType != EventType.Team && model.GameSave.calendar.classifications[j].eventType != EventType.Team)
                    {
                        competitorIDsof2ndPlace = eventResults[i].finalResults[1];
                        actualIDof2ndPlace = eventResults[i].competitorIds[competitorIDsof2ndPlace];
                        // UnityEngine.Debug.Log("secondPlacesCounter eventResults[i] " + i + " classificationIndex: " + j + " competitorIDsofWinner: " + competitorIDsof2ndPlace + " actualIDofWinner " + actualIDof2ndPlace);

                        if (actualIDof2ndPlace == jumper)
                        {

                            secondPlacesCount++;
                            // UnityEngine.Debug.Log("OD CountSecondPlace just added second place! eventID i:" + i + " competitorsFetch name " + competitorsFetch[jumper].Item1 + " actualIDof2ndPlace " + actualIDof2ndPlace + " eventResults[i].results[jumper].Rank:  " + eventResults[i].results[jumper].Rank);

                            for (int k = 0; k < samePodiumPlacesList.Count; k++)
                            {
                                if (samePodiumPlacesList[k].EventIndex == i && samePodiumPlacesList[k].PositionInQuestion == 1)
                                {
                                    secondPlacesCount--;
                                    //firstPlacesCount++;
                                    samePodiumPlacesList[k].JumperName = competitorsFetch[jumper].Item1;
                                    samePodiumPlacesList[k].ClassificationIndex = j;
                                    UnityEngine.Debug.Log("item.Item1 == i && item.Item2 == 1 FIRED!!!!" + competitorsFetch[jumper].Item1 + " ID : " + jumper);
                                }
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

                            for (int k = 0; k < samePodiumPlacesList.Count; k++)
                            {
                                if (samePodiumPlacesList[k].EventIndex == i && samePodiumPlacesList[k].PositionInQuestion == 2)
                                {
                                    thirdPlacesCount--;
                                    //firstPlacesCount++;
                                    samePodiumPlacesList[k].JumperName = competitorsFetch[jumper].Item1;
                                    samePodiumPlacesList[k].ClassificationIndex = j;
                                    UnityEngine.Debug.Log("item.Item2 == i && item.Item3 == 1 FIRED!!!!" + competitorsFetch[jumper].Item1 + " ID : " + jumper);
                                }
                            }


                        }


                    }

                }
            }
            return thirdPlacesCount;
        }



        private void CountFourthPlaces()
        {
            int fourthPlacesCount;
            var index = view.CurrentClassificationIndex;
            var eventCount = model.GameSave.resultsContainer.eventIndex;
            var eventResults = model.GameSave.resultsContainer.eventResults;
            int competitorIDsof4thPlace;
            int actualIDof4thPlace;
            thirdPlacesCount = 0;

            for (int h = 0; h<competitorsFetch.Count;h++) { 
            for (int i = 0; i < eventCount; i++)
            {
                for (int j = 0; j < model.GameSave.calendar.events[i].classifications.Count; j++)
                {
                    if (model.GameSave.calendar.events[i].eventType != EventType.Team && model.GameSave.calendar.classifications[j].eventType != EventType.Team)
                    {
                        competitorIDsof4thPlace = eventResults[i].finalResults[3];
                        actualIDof4thPlace = eventResults[i].competitorIds[competitorIDsof4thPlace];
                        //UnityEngine.Debug.Log("thirdPlacesCounter eventResults[i] " + i + " classificationIndex: " + j + " competitorIDsofWinner: " + competitorIDsof3rdPlace + " actualIDofWinner " + actualIDof3rdPlace);

                        if (actualIDof4thPlace == h)
                        {

                            for (int k = 0; k < samePodiumPlacesList.Count; k++)
                            {
                                if (samePodiumPlacesList[k].EventIndex == i && samePodiumPlacesList[k].PositionInQuestion == 3)
                                {
                                    //firstPlacesCount++;
                                    samePodiumPlacesList[k].JumperName = competitorsFetch[h].Item1;
                                    samePodiumPlacesList[k].ClassificationIndex = j;
                                    UnityEngine.Debug.Log("item.Item3 == i && item.Item4 == 1 FIRED!!!!" + competitorsFetch[h].Item1 + " ID : " + h);
                                }
                            }


                        }}


                    }

                }
            }
        }



        private List<UpdatePodiumsItem> CheckForMultipleJumpersWhoTookSamePodiumPlace()
        {
            var eventResults = model.GameSave.resultsContainer.eventResults;
            var eventCount = model.GameSave.resultsContainer.eventIndex;
            var list = new List<Tuple<int, decimal, decimal, decimal, decimal>>();
            //Kolejne pozycje to numer konkurs
            var samePlaces = new List<UpdatePodiumsItem>();
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

            for (int j = 0; j < eventCount; j++)
            {
                if (list[j].Item2 == list[j].Item3)
                {
                    UnityEngine.Debug.Log($"W zawodach o indeksie " + j + "by³o dwóch zwyciêzców!");
                    samePlaces.Add(new UpdatePodiumsItem(j, 1, 999, null));
                }
                else if (list[j].Item3 == list[j].Item4)
                {
                    samePlaces.Add(new UpdatePodiumsItem(j, 2, 999, null));
                }
                else if (list[j].Item4 == list[j].Item5)
                {
                    samePlaces.Add(new UpdatePodiumsItem(j, 3, 999, null));
                }


            }
            /* foreach (UpdatePodiumsItem item in samePlaces)
             {
                 UnityEngine.Debug.Log($"samePlaces item1: {item.it} item2: {item.Item2}");
             }*/
            return samePlaces;
        }

        public void UpdateResults()
        {
            CountFourthPlaces();
            for (int i = 0; i < samePodiumPlacesList.Count; i++)
            {
                {
                    UnityEngine.Debug.Log(samePodiumPlacesList[i].JumperName);
                    UnityEngine.Debug.Log("ClassificationIndex: " + samePodiumPlacesList[i].ClassificationIndex);
                    UnityEngine.Debug.Log("EventIndex: " + samePodiumPlacesList[i].EventIndex);
                }
            }
            foreach (ResultsListItem item in view.ResultsListController.Results)
            {
                for (int i = 0; i < samePodiumPlacesList.Count; i++)
                {
                    {
                        if(item.name == samePodiumPlacesList[i].JumperName && samePodiumPlacesList[i].PositionInQuestion == 1 && view.CurrentClassificationIndex == samePodiumPlacesList[i].ClassificationIndex)
                        {
                            
                            item.style++;
                        }

                        if (item.name == samePodiumPlacesList[i].JumperName && samePodiumPlacesList[i].PositionInQuestion == 2 && view.CurrentClassificationIndex == samePodiumPlacesList[i].ClassificationIndex)
                        {
                            UnityEngine.Debug.Log("SUKCES KUR£A!!!!!!!!!!!!");
                            item.previousRoundStyle++;
                        }

                        if (item.name == samePodiumPlacesList[i].JumperName && samePodiumPlacesList[i].PositionInQuestion == 3 && view.CurrentClassificationIndex == samePodiumPlacesList[i].ClassificationIndex)
                        {
                            UnityEngine.Debug.Log("SUKCES KUR£A!!!!!!!!!!!!");
                            item.previousRoundDistance++;
                        }
                    }
                }
                view.ResultsListController.Results = view.ResultsListController.Results.ToList();
            }

        }
    }
}
