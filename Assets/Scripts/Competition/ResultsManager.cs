using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.Competition.Runtime;
using OpenSkiJumping.Jumping;
using OpenSkiJumping.Scripts2025;
using OpenSkiJumping.New;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;
using OpenSkiJumping.UI.ListView;
using System.Collections;

namespace OpenSkiJumping.Competition
{
    public interface IResultsManager
    {
        List<Participant> OrderedParticipants { get; }
        Result[] Results { get; }

        Result[] ResultsDeepCopy { get; }

        int[] LastRank { get; }
        List<int> StartList { get; }
        int StartListIndex { get; }
        int SubroundIndex { get; }
        int RoundIndex { get; }
        void RegisterJump(IJumpData jumpData, GameplayExtension gameplayExtension);

        void RegisterCPUJump(IJumpData jumpData, GameplayExtension gameplayExtension);
        void SubroundInit();
        void RoundInit();
        bool SubroundFinish();
        bool RoundFinish();
        bool JumpFinish();

        SortedList<(int state, decimal points, int bib), int> GetFinalResultsAccessible();
        Result GetResultByRank(int rank);
        int GetIdByRank(int rank);
        JumpResults GetResultById(int primaryId, int secondaryId);
        int GetCurrentCompetitorId();
        int GetCurrentCompetitorLocalId();
        int GetCurrentJumperId();

        int GetRoundNumber();
        int CompetitorRank(int id);
        IEnumerable<(int, decimal)> GetPoints(ClassificationInfo classificationInfo);
        EventInfo EventInfo { get; }
        void UpdateEventResults(EventResults eventResults);
    }

    public class ResultsManager : IResultsManager
    {
        private readonly IHillInfo hillInfo;

        private SortedList<(decimal points, int bib, int round), int> allRoundResults;
        private SortedList<(int state, decimal points, int bib), int> finalResults;
        public SortedList<(int state, decimal points, int bib), int> finalResultsAccessible;
        private SortedList<(decimal points, int bib), int> losersResults;
        private List<ResultData> resultsSnapshot;
        private int[] initGates;

        private int competitorsCount;
        private int[] koState;
        private int maxBib;
        private int maxLosers;
        private int roundsCount;
        private int subRoundsCount;


        public ResultsManager(EventInfo eventInfo, List<Participant> orderedParticipants, IHillInfo hillInfo)
        {
            EventInfo = eventInfo;
            OrderedParticipants = orderedParticipants;
            this.hillInfo = hillInfo;
            initGates = new int[eventInfo.roundInfos.Count];

            InitializeValues();
        }


        private void InitializeValues()
        {
            competitorsCount = OrderedParticipants.Count;
            Results = new Result[competitorsCount];
            LastRank = new int[competitorsCount];
            roundsCount = EventInfo.roundInfos.Count;
            subRoundsCount = EventInfo.eventType == EventType.Individual ? 1 : 4;

            for (var index = 0; index < Results.Length; index++)
            {
                Results[index] = new Result();
                var item = Results[index];
                item.TotalResults = new decimal[subRoundsCount];
                item.Results = new JumpResults[subRoundsCount];
                for (var i = 0; i < subRoundsCount; i++) item.Results[i] = new JumpResults();

                item.Bibs = new int[roundsCount];
            }

            finalResults =
                new SortedList<(int state, decimal points, int bib), int>(
                    Comparer<(int state, decimal points, int bib)>.Create(finalResultsComp));
            allRoundResults =
                new SortedList<(decimal points, int bib, int round), int>(
                    Comparer<(decimal points, int bib, int round)>.Create(allRoundResultsComp));
            losersResults =
                new SortedList<(decimal points, int bib), int>(
                    Comparer<(decimal points, int bib)>.Create(losersResultsComp));

            foreach (var kvp in finalResults)
            {
                finalResultsAccessible.Add(kvp.Key, kvp.Value);
            }
            koState = new int[competitorsCount];
        }

        public int StartListIndex { get; private set; }
        public int SubroundIndex { get; private set; }
        public int RoundIndex { get; private set; }

        public List<int> StartList { get; private set; }
        public List<Participant> OrderedParticipants { get; }

        public Result[] Results { get; private set; }

        public Result[] ResultsDeepCopy { get; private set; }
        public int[] LastRank { get; private set; }

        public void SubroundInit()
        {
            var currentRoundInfo = EventInfo.roundInfos[RoundIndex];
            IEnumerable<int> tmp;

            //first sub-round
            if (RoundIndex == 0 && SubroundIndex == 0)
            {
                tmp = Enumerable.Range(0, competitorsCount).Reverse();
            }
            else
            {
                tmp = currentRoundInfo.useOrdRank[SubroundIndex]
                    ? finalResults.Select(item => item.Value).OrderBy(item => item).Reverse()
                    : finalResults.Select(item => item.Value).Reverse();
            }

            for (var i = 0; i < competitorsCount; i++) koState[i] = 0;

            var tmpList = tmp.ToList();
            finalResults.Clear();

            if (currentRoundInfo.roundType == RoundType.Normal)
            {
                StartList = tmpList;
                return;
            }

            StartList = Enumerable.Range(0, tmpList.Count).Select(it => tmpList[KOIndex(it, tmpList.Count)]).ToList();
            maxLosers = Math.Max(0, currentRoundInfo.outLimit - (StartList.Count + 1) / 2);
        }

        public void RoundInit()
        {
            var currentRoundInfo = EventInfo.roundInfos[RoundIndex];

            //first round
            if (RoundIndex == 0)
            {
                for (var i = 0; i < competitorsCount; i++)
                    Results[i].Bibs[RoundIndex] = currentRoundInfo.reversedBibs ? i + 1 : competitorsCount - i;
            }
            //reassign bibs
            else if (currentRoundInfo.reassignBibs)
            {
                for (var i = 0; i < finalResults.Count; i++)
                {
                    var it = finalResults.Values[i];
                    if (currentRoundInfo.reversedBibs)
                        Results[it].Bibs[RoundIndex] = i + 1;
                    else
                        Results[it].Bibs[RoundIndex] = finalResults.Count - i;
                }
            }
            //bibs from previous round
            else
            {
                for (var i = 0; i < finalResults.Count; i++)
                {
                    var id = finalResults.Values[i];
                    var lastRoundBib = Results[id].Bibs[RoundIndex - 1];
                    Results[id].Bibs[RoundIndex] = lastRoundBib;
                }
            }
        }




        public bool JumpFinish()
        {
            StartListIndex++;
            return StartListIndex < StartList.Count;
        }

        public Result GetResultByRank(int rank)
        {
            return Results[allRoundResults.Values[rank]];
        }

        public int GetIdByRank(int rank)
        {
            if (rank < 0 || rank >= finalResults.Count)
            {
                UnityEngine.Debug.LogError($"Invalid rank: {rank}. finalResults.Count: {finalResults.Count}");
                return -1; // Or some other fallback value
            }

            // Create a copy of the SortedList
            finalResultsAccessible = new SortedList<(int state, decimal points, int bib), int>(finalResults);

            return finalResults.Values[rank];
        }

        /* public int GetNHS()
         {
             var id = GetCurrentJumperId();

         }*/

        public JumpResults GetResultById(int primaryId, int secondaryId)
        {
            return Results[primaryId].Results[secondaryId];
        }

        public int GetCurrentCompetitorId() => OrderedParticipants[StartList[StartListIndex]].id;
        public int GetCurrentCompetitorLocalId() => StartList[StartListIndex];
        public int GetCurrentJumperId() => OrderedParticipants[StartList[StartListIndex]].competitors[SubroundIndex];


        public bool SubroundFinish()
        {
            LastRank = Results.Select(item => item.Rank).ToArray();
            UnityEngine.Debug.Log($"Subround {SubroundIndex} is being finished. LastRank[0] value: {LastRank[0]} FinalResults.count {finalResults.Count}");

            finalResultsAccessible = new SortedList<(int state, decimal points, int bib), int>(finalResults.Comparer);
            foreach (var kvp in finalResults)
            {
                finalResultsAccessible.Add(kvp.Key, kvp.Value);
            }

            ResultsDeepCopy = Results
                .Select(result => new Result(result))
                .OrderByDescending(result => result.Rank)
                .ToArray();


            SubroundIndex++;
            StartListIndex = 0;
            return SubroundIndex < subRoundsCount;
        }


        public SortedList<(int state, decimal points, int bib), int> GetFinalResultsAccessible()
        {
            return finalResultsAccessible;
        }

        public bool RoundFinish()
        {
            var currentRoundInfo = EventInfo.roundInfos[RoundIndex];

            switch (currentRoundInfo.outLimitType)
            {
                case LimitType.Normal:
                    var lastIndex = Math.Min(finalResults.Keys.Count, currentRoundInfo.outLimit) - 1;
                    if (currentRoundInfo.roundType == RoundType.KO)
                    {
                        var it = Math.Min(currentRoundInfo.outLimit, finalResults.Count);
                        while (it < finalResults.Count && finalResults.Keys[it].state == 0 &&
                               finalResults.Keys[it - 1].points == finalResults.Keys[it].points) it++;

                        var stop = it;
                        it = finalResults.Count - 1;
                        while (stop <= it)
                        {
                            finalResults.RemoveAt(it);
                            it--;
                        }
                    }
                    else
                    {
                        var minPts = finalResults.Keys[lastIndex].points;
                        var it = finalResults.Count - 1;
                        while (finalResults.Keys[it].points < minPts)
                            finalResults.RemoveAt(it--);
                    }

                    break;
                case LimitType.Exact:
                    for (var i = finalResults.Count - 1; i >= currentRoundInfo.outLimit; i--)
                        finalResults.RemoveAt(i);
                    break;
            }


            RoundIndex++;
            SubroundIndex = 0;
            return RoundIndex < roundsCount;
        }

        public void RegisterJump(IJumpData jumpData, GameplayExtension gameplayExtension)
        {
        
                // Handle disableJudgesMarks
                var currentRoundInfo = EventInfo.roundInfos[RoundIndex];
                if (currentRoundInfo.disableJudgesMarks)
                    for (var i = 0; i < jumpData.JudgesMarks.Length; i++)
                        jumpData.JudgesMarks[i] = 0m;

                //Set init gate for round
                if (StartListIndex == 0 && SubroundIndex == 0)
                {
                    initGates[RoundIndex] = jumpData.Gate;
                    jumpData.InitGate = jumpData.Gate;
                }

                var jump = EventProcessor.GetJumpResult(jumpData, hillInfo, currentRoundInfo.gateCompensation, currentRoundInfo.windCompensation);
                if (RoundIndex > 0 || SubroundIndex > 0) RemoveFromAllRoundResults();

            jump.actualGate = (decimal)gameplayExtension.storeGate;
            
            AddResult(StartList[StartListIndex], SubroundIndex, jump);
            AddToAllRoundResults();
            AddToFinalResults();
            
        }

        public void RegisterCPUJump(IJumpData jumpData, GameplayExtension gameplayExtension)
        {

            var currentRoundInfo = EventInfo.roundInfos[RoundIndex];
            /*if (currentRoundInfo.disableJudgesMarks)
            {
                for (var i = 0; i < jumpData.JudgesMarks.Length; i++)
                    jumpData.JudgesMarks[i] = 0m;
            }*/

            //Set init gate for round
            if (StartListIndex == 0 && SubroundIndex == 0)
            {
                initGates[RoundIndex] = jumpData.Gate;
                jumpData.InitGate = jumpData.Gate;
            }

            JumpResult cpuJump = new JumpResult();
            cpuJump.distance = jumpData.CPUDistance;
            //Trzeba przeniesc przypisywanie jumperSkill na wczeœniejszy moment niz Gate() w jumperContorller
            cpuJump.distance = CalculateFinalCPUDistance(jumpData.CPUDistance, jumpData.JumperSkill, gameplayExtension.modifierCPURandomnessLevel);
            jumpData.Distance = cpuJump.distance;
            // cpuJump.judgesMarks = CalculateJudgesMarks(cpuJump.judgesMarks, cpuJump.distance, hillInfo, jumpData.JumperSkill);

            decimal[] points = { 0, 0, 0, 0, 0 };
            points = CalculateCPUJudgesMarks(points, jumpData.Distance, hillInfo, jumpData.JumperSkill); 
            cpuJump.judgesMarks = points;
            jumpData.JudgesMarks = points;
            cpuJump.actualGate = (decimal)gameplayExtension.storeGate;
            if (currentRoundInfo.disableJudgesMarks)
            {
                for (var i = 0; i < jumpData.JudgesMarks.Length; i++)
                    jumpData.JudgesMarks[i] = 0m;
            }
            cpuJump = EventProcessor.GetJumpResult(jumpData, hillInfo, currentRoundInfo.gateCompensation, currentRoundInfo.windCompensation);



            if (RoundIndex > 0 || SubroundIndex > 0) RemoveFromAllRoundResults();
            cpuJump.actualGate = (decimal)gameplayExtension.storeGate;
            AddResult(StartList[StartListIndex], SubroundIndex, cpuJump);
            AddToAllRoundResults();
            AddToFinalResults();

        }

        public decimal[] CalculateCPUJudgesMarks(decimal [] marks, decimal distance, IHillInfo hillInfo, int skill)
        {
            bool crashed = false;

            if (Random.Range(0, 100) > 97)
            {
                crashed = true;
                UnityEngine.Debug.Log("CRASHED TRUE!");
            }


            float skillModifier = 1;
           // UnityEngine.Debug.Log("CalculateJudgesMark skillModifier: " + skillModifier + " skill: " + skill + " distance " + distance + " hillinfo.GetHS() " + hillInfo.GetHS());
            if (skill != 1)
                {
                    skillModifier = (101 - (float)skill) / 100;
                }
          //  UnityEngine.Debug.Log("CalculateJudgesMark skillModifier: " + skillModifier);

            for (int i = 0; i < marks.Length; i++)
              {

                if (!crashed)
                {
                    float singleMark;
                    if (distance > (decimal)hillInfo.GetHS() * 1.06m)
                    {
                            singleMark = Random.Range(10f, 12f);
                        crashed = true;
                        UnityEngine.Debug.Log("CRASHED CAUSE OVER 1.06!!");

                    }
                    else if (distance > (decimal)hillInfo.GetHS() * 1.04m)
                    {
                        if (Random.Range(0, 100) > 70)
                        {
                            singleMark = Random.Range(10f, 12f);
                        }
                        else
                        {
                            singleMark = Random.Range(15f, 19f);
                        }
                    }
                    else if (distance > (decimal)hillInfo.GetHS() * 1.02m)
                    { 

                        if (Random.Range(0, 100) > 85)
                        {
                            singleMark = Random.Range(10, 12);
                        }
                        else
                        {
                            singleMark = Random.Range(16f, 19f);
                        }
                    }
                    else if (distance > (decimal)hillInfo.GetHS() * 0.95m)
                    {

                        singleMark = Random.Range(18f, 20f);
                    }
                    else if (distance > (decimal)hillInfo.GetHS() * 0.85m)
                    {

                        singleMark = Random.Range(17f, 19f);
                    }
                    else if (distance > (decimal)hillInfo.GetHS() * 0.75m)
                    {

                        singleMark = Random.Range(16f, 18f);
                    }
                    else
                    {

                        singleMark = Random.Range(15f, 17f);
                    }
                    
                    singleMark -= skillModifier;
                    //UnityEngine.Debug.Log("singleMark przed rounding" + singleMark);
                    marks[i] = Math.Round((decimal)singleMark * 2, MidpointRounding.AwayFromZero) / 2;
                }
                else
                {
                    marks[i] = Random.Range(10, 12);
                }
                
              }

            return marks;
        }

        public decimal CalculateFinalCPUDistance(decimal distance, int skill, float sliderRandomnessModifier)
        {
            float modifier = 1;
            if (sliderRandomnessModifier == 0)
            {
                sliderRandomnessModifier = 20;
            }

            //UnityEngine.Debug.Log("sliderRandomnessModifier " + sliderRandomnessModifier);

            //UnityEngine.Debug.Log("Od ResultsManager jumpData.JumperSKill: " + skill);
            //UnityEngine.Debug.Log("Od ResultsManager jumpData.CPUDistance przed zmiana skilla: " + distance);

            if (skill > 90)
            {

                modifier += (((float)skill - 90f) / 180f);
                //UnityEngine.Debug.Log("Od ResultsManager CPU Distance modifeir: " + modifier);

            }

            if (skill < 90)
            {

                modifier -= ((90f - (float)skill) / 180f);
                //UnityEngine.Debug.Log("Od ResultsManager CPU Distance modifeir: " + modifier);
            }
            // UnityEngine.Debug.Log("Od ResultsManager Distance BEFORE Random: " + distance * (decimal)modifier);

            //Pierwotnie 0,06f, 0,02f
            modifier += Random.Range(-0.03f * (sliderRandomnessModifier / 10f), 0.015f * (sliderRandomnessModifier / 10f));
            if (Random.Range(0, 100) > 100 - (sliderRandomnessModifier / 20f))
            {


                //pierwotnie 0,08f, 0,03f
                modifier += Random.Range(-0.04f * (sliderRandomnessModifier / 10f), 0.2f * (sliderRandomnessModifier / 10f));
                //   UnityEngine.Debug.Log("Od ResultsManager EXTRA RANDOM FACTOR! New modifier: " + modifier);
            }
            //UnityEngine.Debug.Log("Od ResultsManager Distance AFTER Random: " + distance * (decimal)modifier);

            if (skill * modifier > 95)
            {
                //UnityEngine.Debug.Log("UWAGA! Po modiefierze baardzo d³ugi skok CPU. Skill: " + skill + "skill * modifier: " + skill*modifier);
                if (skill > 90)
                {
                    while (skill * modifier > 95)
                    {
                        modifier *= Random.Range(0.9f,0.95f);
                    }
                }
                else
                {
                    while (skill * modifier > 90)
                    {
                        modifier *= 0.95f;
                    }
                }

              // UnityEngine.Debug.Log("Po naprawie skill * modifier: " + skill * modifier + " sam modifier: " + modifier);
            }



            while(modifier > 1.1f)
            {
                UnityEngine.Debug.Log("COS SIE JEEEEBLO! Modifier wiekszy niz 1.1. Skok: " + (float)distance * modifier + " skill: " + skill + " modifier " + modifier + " hillsize " + hillInfo.GetHS());
                modifier *= Random.Range(skill / 100f, (skill / 100f) + 0.1f);
                if (skill > 90)
                {
                    modifier *= 0.9f;
                }
                UnityEngine.Debug.Log("COS SIE JEEEEBLO! Po zmianie modifiera wiekszego ni¿ 1.1. Skok: " + (float)distance * modifier + " skill: " + skill + " modifier " + modifier + " hillsize " + hillInfo.GetHS());
            }

            /*

            while (((float)distance * modifier) > (hillInfo.GetHS() * 1.09f))
            {
                UnityEngine.Debug.Log("COS SIE JEEEEEBLO! Bardzo dlugi skok!: " + (float)distance*modifier + " skill: " + skill + " modifier " + modifier + " hillsize " + hillInfo.GetHS());
                
                modifier *= Random.Range(skill/100f, (skill/100f)+0.1f);
                if(skill > 90)
                {
                    modifier *= 0.9f;
                }

                UnityEngine.Debug.Log("COS SIE JEEEEEBLO! D³ugoœæ zbugowanego skoku po zmianach: " + (float)distance * modifier + "skill: " + skill + " modifier " + modifier + " hillsize " + hillInfo.GetHS());

            }

            if(modifier > 1.1f)
            {
                UnityEngine.Debug.Log("COS SIE JEEEEBLO! Modifier wciaz wiekszy niz 1.1. Skok: " + (float)distance * modifier + " skill: " + skill + " modifier " + modifier + " hillsize " + hillInfo.GetHS());
                modifier *= Random.Range(skill / 100f, 0.99f);
                UnityEngine.Debug.Log("COS SIE JEEEEBLO! Po zmianie modifiera wiekszego ni¿ 1.1. Skok: " + (float)distance * modifier + " skill: " + skill + " modifier " + modifier + " hillsize " + hillInfo.GetHS());
            }


            */


            /*
            if (((float)distance * modifier) > (hillInfo.GetHS() * 1.08f))
            {
                UnityEngine.Debug.Log("COS SIE JEEEEEBLO! Bardzo dlugi skok!: " + (float)distance + " skill: " + skill + " modifier " + modifier + " hillsize " + hillInfo.GetHS());
                distance = 999m;
            }*/

            return Math.Round((distance * (decimal)modifier) * 2, MidpointRounding.AwayFromZero) / 2;
        }





        private void AddResult(int primaryIndex, int secondaryIndex, JumpResult jump)
        {
            Results[primaryIndex].Results[secondaryIndex].results.Add(jump);
            Results[primaryIndex].TotalResults[secondaryIndex] =
            Results[primaryIndex].Results[secondaryIndex].results.Sum(item => item.totalPoints);
            Results[primaryIndex].Distance = jump.distance;


            
            Results[primaryIndex].TotalPoints = Results[primaryIndex].TotalResults.Sum();
//Results[primaryIndex].TotalResults[secondaryIndex] i Results[primaryIndex].TotalPoints) to wynik z obu rund. Jeœli jest to runda pierwsza - tylko z pierwszej;
        }

        public int CompetitorRank(int id)
        {
            var key = (koState[id], Results[id].TotalPoints, -1);
            int lo = 0, hi = finalResults.Count;
            while (lo < hi)
            {
                var index = lo + (hi - lo) / 2;
                var el = finalResults.Keys[index];
                if (finalResults.Comparer.Compare(el, key) >= 0)
                    hi = index;
                else
                    lo = index + 1;
            }

            return hi + 1;
        }

        private void AddToFinalResults()
        {
            if (EventInfo.roundInfos[RoundIndex].roundType == RoundType.KO && StartListIndex % 2 == 1)
            {
                AddSecondKOJumper();
            }
            else
            {
                var id = StartList[StartListIndex];
                var bibCode = GetBibCode(Results[id].Bibs[RoundIndex]);
                finalResults.Add((0, Results[id].TotalPoints, bibCode), id);
            }
        }

        public int roundNumber = 0;

        public int GetRoundNumber()
        {
            return roundNumber;
        }


        private void AddToAllRoundResults()
        {
            var competitorId = StartList[StartListIndex];
            var subroundNum = RoundIndex * subRoundsCount + SubroundIndex;
            roundNumber = subroundNum;
            var bibCode = GetBibCode(Results[competitorId].Bibs[RoundIndex]);
            var jumpResults = GetResultById(competitorId, SubroundIndex);

          //  UnityEngine.Debug.Log("allRoundResults.Add((Results[competitorId].TotalPoints: " + Results[competitorId].TotalPoints + " bibCode: " + bibCode + " subroundNum " + subroundNum + " subround index " + SubroundIndex + " competitorID: " + competitorId + "Results[competitorId].TotalPoints" + Results[competitorId].TotalResults);
            allRoundResults.Add((Results[competitorId].TotalPoints, bibCode, subroundNum), competitorId);



            // Write down the distance that will be display as previous round distance
            if (subroundNum != 0 && subRoundsCount == 1)
            {
                Results[competitorId].PreviousRoundDistance = jumpResults.results[RoundIndex - 1].distance;
                Results[competitorId].PreviousRoundStyle = jumpResults.results[RoundIndex - 1].judgesTotalPoints;
                Results[competitorId].PreviousRoundGate = jumpResults.results[RoundIndex - 1].actualGate;
            }

            Results[competitorId].Style = jumpResults.results[RoundIndex].judgesTotalPoints;
            Results[competitorId].ActualGate = jumpResults.results[RoundIndex].actualGate;

            //Write down the name of the jumper
            Results[competitorId].CurrentCompetitorId = OrderedParticipants[competitorId].id;

            // Update rank
            for (var i = 0; i < Math.Min(competitorsCount, allRoundResults.Count); i++)
            {


                    //UnityEngine.Debug.Log("Debugging ResultsManager i: " + i + " competitorCount " + competitorsCount + " allRoundResults.Count " + allRoundResults.Count);
                    if (i > 0 && allRoundResults.Keys[i].points == allRoundResults.Keys[i - 1].points)
                        Results[allRoundResults.Values[i]].Rank = Results[allRoundResults.Values[i - 1]].Rank;
                    else
                        Results[allRoundResults.Values[i]].Rank = i + 1;
                


            }
            ;






        }

        private void RemoveFromAllRoundResults()
        {
            var competitorId = StartList[StartListIndex];
            var subroundNum = RoundIndex * subRoundsCount + SubroundIndex - 1;
            var bibRoundIndex = SubroundIndex > 0 ? RoundIndex : RoundIndex - 1;
            var bibCode = GetBibCode(Results[competitorId].Bibs[bibRoundIndex]);
            allRoundResults.Remove((Results[competitorId].TotalPoints, bibCode, subroundNum));
        }

        private void AddSecondKOJumper()
        {
            var id1 = StartList[StartListIndex - 1];
            var id2 = StartList[StartListIndex];
            var bibCode1 = GetBibCode(Results[id1].Bibs[RoundIndex]);
            var bibCode2 = GetBibCode(Results[id2].Bibs[RoundIndex]);
            var result1 = Results[id1].TotalPoints;
            var result2 = Results[id2].TotalPoints;

            finalResults.Remove((0, result1, bibCode1));

            int loserId = id1, winnerId = id2;
            int loserBib = bibCode1, winnerBib = bibCode2;

            if (result1 > result2)
            {
                winnerId = id1;
                winnerBib = bibCode1;
                loserId = id2;
                loserBib = bibCode2;
            }

            finalResults.Add((0, Results[winnerId].TotalPoints, winnerBib), winnerId);

            Results[loserId].Results[SubroundIndex].results[RoundIndex].state = JumpResultState.KoLoser;
            losersResults.Add((Results[loserId].TotalPoints, loserBib), loserId);
            var loserRank = losersResults.IndexOfKey((Results[loserId].TotalPoints, loserBib));

            //lost
            if (loserRank >= maxLosers)
            {
                koState[loserId] = 1;
                finalResults.Add((1, Results[loserId].TotalPoints, loserBib), loserId);
            }
            //lucky loser
            else
            {
                //remove last lucky loser
                if (losersResults.Count > maxLosers)
                {
                    var (lastLoserPoints, lastLoserBib) = losersResults.Keys[maxLosers];
                    var lastLoserId = losersResults.Values[maxLosers];
                    koState[lastLoserId] = 1;
                    Results[loserId].Results[SubroundIndex].results[RoundIndex].state = JumpResultState.KoLoser;

                    finalResults.Remove((0, lastLoserPoints, lastLoserBib));
                    finalResults.Add((1, lastLoserPoints, lastLoserBib), lastLoserId);
                }

                finalResults.Add((0, Results[loserId].TotalPoints, loserBib), loserId);
            }
        }

        private readonly Comparison<(int state, decimal points, int bib)> finalResultsComp = (x, y) =>
            x.state == y.state
                ? x.points == y.points
                    ? x.bib.CompareTo(y.bib)
                    : y.points.CompareTo(x.points)
                : x.state.CompareTo(y.state);

        private readonly Comparison<(decimal points, int bib, int round)> allRoundResultsComp = (x, y) =>
            x.points == y.points
                ? x.bib == y.bib
                    ? y.round.CompareTo(x.round)
                    : x.bib.CompareTo(y.bib)
                : y.points.CompareTo(x.points);

        private readonly Comparison<(decimal points, int bib)> losersResultsComp = (x, y) =>
            x.points == y.points
                ? x.bib.CompareTo(y.bib)
                : y.points.CompareTo(x.points);

        private int GetBibCode(int bib) =>
            EventInfo.roundInfos[RoundIndex].reversedBibs ? bib : competitorsCount - bib;

        private static int KOIndex(int index, int length)
        {
            var halfLen = length / 2;
            var halfIndex = index / 2;

            if (index == 2 * halfLen) return index;
            if (index % 2 == 0)
                return halfLen - 1 - halfIndex;
            return halfLen + halfIndex;
        }

        public IEnumerable<(int, decimal)> GetPoints(ClassificationInfo classificationInfo)
        {
            return classificationInfo.eventType == EventType.Individual
                ? GetIndividualPoints(classificationInfo)
                : GetTeamPoints(classificationInfo);
        }

        public EventInfo EventInfo { get; }

        public void UpdateEventResults(EventResults eventResults)
        {
            eventResults.competitorIds = OrderedParticipants.Select(it => it.id).ToList();
            eventResults.results = Results.ToList();
            eventResults.allroundResults = allRoundResults.Select(it => it.Value).ToList();
            eventResults.finalResults = finalResults.Select(it => it.Value).ToList();
        }

        private IEnumerable<(int, decimal)> GetIndividualPoints(ClassificationInfo classificationInfo)
        {
            IPointsGiver pointsGiver;
            if (classificationInfo.classificationType == ClassificationType.Place)
                pointsGiver = new PlacePointsGiver();
            else
                pointsGiver = new PointsPointsGiver();

            if (EventInfo.eventType == EventType.Individual)
            {
                UnityEngine.Debug.Log("   if (EventInfo.eventType == EventType.Individual) GetIndividualPoints running");

                return Results.Select((val, index) =>
                    (OrderedParticipants[index].id, pointsGiver.GetPoints(classificationInfo, val, 0)));

            }
            
            //EventInfo.eventType == EventType.Team
            
            var res = new List<(int, decimal)>();
            for (var i = 0; i < Results.Length; i++)
            {
                res.AddRange(Results[i].TotalResults.Select((t, j) => (OrderedParticipants[i].competitors[j], t)));
                UnityEngine.Debug.Log("for (var i = 0; i < Results.Length; i++) running. i: " + i);
            }

            if (classificationInfo.classificationType == ClassificationType.Points)
                return res;
            res = res.OrderByDescending(it => it.Item2).ToList();

            Result x;
            
            var indRank = new int[res.Count];
            indRank[0] = 1;
            for (var i = 1; i < indRank.Length; i++)
            {
                if (res[i - 1].Item2 == res[i].Item2) indRank[i] = indRank[i - 1];
                else indRank[i] = i + 1;
            }

            UnityEngine.Debug.Log("            return res.Select((it, ind) => (it.Item1, PointsUtils.GetPlacePoints(classificationInfo, ind, 0)));");
            return res.Select((it, ind) => (it.Item1, PointsUtils.GetPlacePoints(classificationInfo, ind, 0)));
                // 0 < indRank[ind] && indRank[ind] < classificationInfo.pointsTables[0].value.Length
                //     ? classificationInfo.pointsTables[0].value[indRank[ind] - 1]
                //     : 0m));
        }

        private IEnumerable<(int, decimal)> GetTeamPoints(ClassificationInfo classificationInfo)
        {
            IPointsGiver pointsGiver;
            if (classificationInfo.classificationType == ClassificationType.Place)
                pointsGiver = new PlacePointsGiver();
            else
                pointsGiver = new PointsPointsGiver();

            if (EventInfo.eventType == EventType.Team)
            {
                return Results.Select((val, index) =>
                    (OrderedParticipants[index].id, pointsGiver.GetPoints(classificationInfo, val, 1)));
            }

            var competitorsByTeam = OrderedParticipants.Select((it, ind) => (it, ind)).ToLookup(it => it.it.teamId);
            return competitorsByTeam.Select(teamMembers => (
                teamMembers.Key,
                (classificationInfo.teamClassificationLimitType == TeamClassificationLimitType.Best
                    ? teamMembers.Select(it => pointsGiver.GetPoints(classificationInfo, Results[it.ind], 0))
                        .OrderByDescending(it => it).Take(classificationInfo.teamCompetitorsLimit)
                    : teamMembers.Select(it => pointsGiver.GetPoints(classificationInfo, Results[it.ind], 0))).Sum()));
        }
    }
}