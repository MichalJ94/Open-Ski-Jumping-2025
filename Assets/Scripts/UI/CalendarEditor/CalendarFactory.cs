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


        public void MigrateEventGuidsIfNeeded()
        {
            bool changed = false;

            // 1. Ensure every event has a GUID
            foreach (var ev in events)
            {
                if (string.IsNullOrEmpty(ev.eventGuid))
                {
                    ev.eventGuid = Guid.NewGuid().ToString("N");
                    changed = true;
                }
            }

            // 2. Build index to guid map
            var indexToGuid = events
                .Select((ev, index) => (index, ev.eventGuid))
                .ToDictionary(x => x.index, x => x.eventGuid);

            // 3. Migrate rank references
            foreach (var ev in events)
            {
                if (ev.ordRankType == RankType.Event && ev.ordRankId >= 0)
                {
                    if (indexToGuid.TryGetValue(ev.ordRankId, out var guid))
                        ev.ordEventGuid = guid;
                    else
                        ev.ordEventGuid = null;
                }

                if (ev.qualRankType == RankType.Event && ev.qualRankId >= 0)
                {
                    if (indexToGuid.TryGetValue(ev.qualRankId, out var guid))
                        ev.qualEventGuid = guid;
                    else
                        ev.qualEventGuid = null;
                }

                if (ev.preQualRankType == RankType.Event && ev.preQualRankId >= 0)
                {
                    if (indexToGuid.TryGetValue(ev.preQualRankId, out var guid))
                        ev.preQualEventGuid = guid;
                    else
                        ev.preQualEventGuid = null;
                }
            }

            if (changed)
                Debug.Log("[Calendar] Event GUID migration completed");
        }


        public void MoveEvent(EventInfo item, int direction)
        {
            var oldIndex = events.IndexOf(item);
            var newIndex = oldIndex + direction;

            if (oldIndex < 0 || newIndex < 0 || newIndex >= events.Count)
                return;

            // Move the event
            events.RemoveAt(oldIndex);
            events.Insert(newIndex, item);

            // Reassign IDs so id == index
            for (int i = 0; i < events.Count; i++)
                events[i].id = i;

            // CRITICAL STEP
            RebuildRankIndicesFromGuids();
        }

        public void AddEvent(EventInfo item)
        {
            item.id = events.Count;
            events.Add(item);
        }

        public bool RemoveEvent(EventInfo item)
        {
            var index = events.IndexOf(item);
            if (index < 0)
                return false;

            // Update references to the removed event
            foreach (var ev in events)
            {
                // ---- ORD RANK ----
                if (ev.ordRankType == RankType.Event)
                {
                    if (ev.ordRankId == index)
                    {
                        ev.ordRankType = RankType.None;
                        ev.ordRankId = 0;
                        ev.ordEventGuid = null;
                    }
                    else if (ev.ordRankId > index)
                    {
                        ev.ordRankId--;
                    }
                }

                // ---- QUAL RANK ----
                if (ev.qualRankType == RankType.Event)
                {
                    if (ev.qualRankId == index)
                    {
                        ev.qualRankType = RankType.None;
                        ev.qualRankId = 0;
                        ev.qualEventGuid = null;
                    }
                    else if (ev.qualRankId > index)
                    {
                        ev.qualRankId--;
                    }
                }

                // ---- PREQUAL RANK ----
                if (ev.preQualRankType == RankType.Event)
                {
                    if (ev.preQualRankId == index)
                    {
                        ev.preQualRankType = RankType.None;
                        ev.preQualRankId = 0;
                        ev.preQualEventGuid = null;
                    }
                    else if (ev.preQualRankId > index)
                    {
                        ev.preQualRankId--;
                    }
                }
            }

            // Remove the event itself
            events.RemoveAt(index);

            // Reassign IDs to keep editor/UI logic intact
            for (var i = index; i < events.Count; i++)
            {
                events[i].id = i;
            }

            return true;
        }

        private void RebuildRankIndicesFromGuids()
        {
            // Build GUID to index lookup
            var guidToIndex = events
                .Where(e => !string.IsNullOrEmpty(e.eventGuid))
                .ToDictionary(e => e.eventGuid, e => e.id);

            foreach (var ev in events)
            {
                // ---- QUAL ----
                if (ev.qualRankType == RankType.Event)
                {
                    if (!string.IsNullOrEmpty(ev.qualEventGuid) &&
                        guidToIndex.TryGetValue(ev.qualEventGuid, out var idx))
                    {
                        ev.qualRankId = idx;
                    }
                    else
                    {
                        ev.qualRankType = RankType.None;
                        ev.qualRankId = 0;
                        ev.qualEventGuid = null;
                    }
                }

                // ---- ORD ----
                if (ev.ordRankType == RankType.Event)
                {
                    if (!string.IsNullOrEmpty(ev.ordEventGuid) &&
                        guidToIndex.TryGetValue(ev.ordEventGuid, out var idx))
                    {
                        ev.ordRankId = idx;
                    }
                    else
                    {
                        ev.ordRankType = RankType.None;
                        ev.ordRankId = 0;
                        ev.ordEventGuid = null;
                    }
                }

                // ---- PREQUAL ----
                if (ev.preQualRankType == RankType.Event)
                {
                    if (!string.IsNullOrEmpty(ev.preQualEventGuid) &&
                        guidToIndex.TryGetValue(ev.preQualEventGuid, out var idx))
                    {
                        ev.preQualRankId = idx;
                    }
                    else
                    {
                        ev.preQualRankType = RankType.None;
                        ev.preQualRankId = 0;
                        ev.preQualEventGuid = null;
                    }
                }
            }
        }



        public void RecalculateEvents()
        {
            MigrateEventGuidsIfNeeded();

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

        public void ValidateData()
        {
            events[0].qualRankId = 0;
            events[0].qualRankType = RankType.None;
            events[0].ordRankId = 0;
            events[0].ordRankType = RankType.None;
            events[0].preQualRankId = 0;
            events[0].preQualRankType = RankType.None;
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