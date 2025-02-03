using System;
using System.Collections.Generic;
using System.Linq;
using OpenSkiJumping.Competition;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.ScriptableObjects;
using UnityEngine;

namespace OpenSkiJumping.UI.CalendarEditor
{
    [Serializable]
    public struct ClassificationData
    {
        public string name;
        public int id;
    }

    [CreateAssetMenu(menuName = "ScriptableObjects/CalendarFactory")]
    public class CalendarFactory : ScriptableObject
    {
        [SerializeField] private FlagsData flagsData;
        [SerializeField] private List<ClassificationData> classificationDataList;
        [SerializeField] private List<ClassificationInfo> classifications;
        [SerializeField] private List<Competitor> competitors;
        [SerializeField] private List<EventInfo> events;

        public List<EventInfo> Events
        {
            get => events;
            set => events = value;
        }

        public List<ClassificationInfo> Classifications => classifications;

        public List<Competitor> Competitors
        {
            get => competitors;
            set => competitors = value;
        }

        public void MoveEvent(EventInfo item, int val)
        {
            var index = Events.IndexOf(item);
            if (index < 0 || index + val < 0 || Events.Count <= index + val) return;
            var buf = Events[index + val];

            // Swap ids
            Events[index].id = index + val;
            Events[index + val].id = index;

            Events[index + val] = Events[index];
            Events[index] = buf;

            // Events[index].name = $"{index + 1} {Events[index].hillId}";
            // Events[index + val].name = $"{index + val + 1} {Events[index].hillId}";
        }

        public void AddEvent(EventInfo item)
        {
            item.id = events.Count;
            events.Add(item);
        }

        public bool RemoveEvent(EventInfo item)
        {
            var index = events.IndexOf(item);
            if (index < 0) return false;

            // Update references to removed event
            // 20250203 - need to update the rank type as well

            /*
             * Kod Jonka
             * 
             * 
             *             foreach (var ev in events)
            {
                if (ev.qualRankId == index) ev.qualRankId = -1; // Reset if it was pointing to the removed event
                else if (ev.qualRankId > index) ev.qualRankId--; // Shift down if the index was higher

                if (ev.ordRankId == index) ev.ordRankId = -1;
                else if (ev.ordRankId > index) ev.ordRankId--;

                if (ev.preQualRankId == index) ev.preQualRankId = -1;
                else if (ev.preQualRankId > index) ev.preQualRankId--;
            }

             * 
             * 
             * */


            foreach (var ev in events)
            {
                if (ev.qualRankId == index)
                {
                    ev.qualRankId = 0;
                    ev.qualRankType = RankType.None;
                } // Reset if it was pointing to the removed event
                else if (ev.qualRankId > index) ev.qualRankId--; // Shift down if the index was higher

                if (ev.ordRankId == index)
                {
                    ev.ordRankId = 0;
                    ev.ordRankType = RankType.None;
                }
                else if (ev.ordRankId > index) ev.ordRankId--;

                if (ev.preQualRankId == index)
                {
                    ev.preQualRankId = 0;
                    ev.preQualRankType = RankType.None;
                }
                else if (ev.preQualRankId > index) ev.preQualRankId--;
            }

            // Remove the event
            events.RemoveAt(index);

            // Reassign event IDs
            for (var i = index; i < events.Count; i++)
            {
                events[i].id = i;
            }

            return true;
        }

        public void RecalculateEvents()
        {
            for (var i = 0; i < events.Count; i++) events[i].id = i;
            var map = classificationDataList.Where(item => item.id >= 0)
                .Select((item, index) => (item, index))
                .ToDictionary(x => x.item.id, x => x.index);

            foreach (var item in events)
            {
                item.classifications =
                    item.classifications.Where(it => map.ContainsKey(it)).Select(it => map[it]).ToList();
                //
               /* if(item.qualRankId >= item.id && item.qualRankType == RankType.Event)
                {
         
                    item.qualRankId = item.id-1;
                    if(item.id == 0)
                    {
                        item.qualRankId = 0;
                        item.qualRankType = RankType.None;
                    }
                }
                if (item.preQualRankId >= item.id && item.preQualRankType == RankType.Event)
                {
                    item.preQualRankId = item.id - 1;
                    if (item.id == 0)
                    {
                        item.preQualRankId = 0;
                        item.preQualRankType = RankType.None;
                    }
                }

                if(item.ordRankId == -1)
                {
                    item.ordRankId = 0;
                    item.preQualRankId = 0;
                    item.qualRankId = 0;
                }

            */
            }

            for (var i = 0; i < classificationDataList.Count; i++)
                classificationDataList[i] = new ClassificationData {name = classifications[i].name, id = i};
        }

        public bool RemoveClassification(ClassificationInfo item)
        {
            var index = classifications.IndexOf(item);
            if (index < 0) return false;
            classifications.RemoveAt(index);
            classificationDataList.RemoveAt(index);

            return true;
        }

        public void AddClassification(ClassificationInfo item)
        {
            classifications.Add(item);
            classificationDataList.Add(new ClassificationData {name = "", id = -1});
        }

        public void LoadClassifications(List<ClassificationInfo> data)
        {
            classifications = data;
            classificationDataList = data.Select((item, index) => new ClassificationData {name = item.name, id = index})
                .ToList();
        }

        public IEnumerable<ClassificationData> GetClassificationDataFromIds(List<int> ids)
        {
            return ids.Select(it => new ClassificationData {name = Classifications[it].name, id = it});
        }

        public IEnumerable<ClassificationData> GetClassificationData()
        {
            return classificationDataList;
        }

        public Calendar CreateCalendar()
        {
            var tmpEvents = Events.ToList();
            var tmpClassifications = Classifications.ToList();
            var tmpCompetitorsIds = competitors.Select(it => it.id).ToList();
            var tmpTeams = competitors.GroupBy(it => it.countryCode).Select(it => new Team
                {
                    countryCode = it.Key, teamName = flagsData.GetEnglishName(it.Key),
                    competitorsIds = it.Select(comp => comp.id).ToList()
                })
                .ToList();
            
            return new Calendar
            {
                events = tmpEvents, classifications = tmpClassifications, competitorsIds = tmpCompetitorsIds,
                teams = tmpTeams
            };
        }
    }
}