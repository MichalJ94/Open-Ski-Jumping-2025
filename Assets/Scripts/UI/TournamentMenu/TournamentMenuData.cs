using System;
using System.Collections.Generic;
using System.Linq;
using OpenSkiJumping.Competition;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.Data;
using OpenSkiJumping.ScriptableObjects;
using UnityEngine;
using EventType = OpenSkiJumping.Competition.EventType;

namespace OpenSkiJumping.UI.TournamentMenu
{
    [Serializable]
    public class ClassificationData
    {
        public ClassificationInfo classification;
        public int priority;
        public int calendarId;
        public bool useBib;
    }

    [Serializable]
    public class CompetitorData
    {
        public int calendarId;
        public Competitor competitor;
        public bool registered;
        public int teamId;

    }

    [Serializable]
    public class TeamData
    {
        public int calendarId;
        public List<CompetitorData> competitors;
        public bool registered;
        public Team team;
        /// <summary>Returns the first <paramref name="count"/> members of the team (the squad that competes).</summary>
        public IEnumerable<Competitor> GetTeamMembers(int count = 4) => competitors.Take(count).Select(it => it.competitor);
    }

    public enum RandomEventType
    {
        SkillChange,
        Injury
    }

    [Serializable]
    public class RandomEventData
    {
        public int competitorId;
        public int skillChange;
        public string localizedPhrase;
        public int eventsInjuredLeft;
        public int eventIndex;

        public RandomEventData(int competitorId, int skillChange, TranslatablePhrase phrase, int injuryDaysLeft, int eventIndex)
        {
            this.competitorId = competitorId;
            this.skillChange = skillChange;
            this.localizedPhrase = phrase != null ? phrase.CurrentValue : null;
            this.eventsInjuredLeft = injuryDaysLeft;
            this.eventIndex = eventIndex;
 
        }

        public string GetLocalizedDescription(string competitorName, string eventInfo = "")
        {
            if (string.IsNullOrEmpty(localizedPhrase))
                return $"{competitorName} skill change: {skillChange} after {eventInfo}";

            return string.Format(localizedPhrase, competitorName, Math.Abs(skillChange), eventInfo);
        }
    }

    [CreateAssetMenu(menuName = "ScriptableObjects/TournamentMenuData")]
    public class TournamentMenuData : ScriptableObject
    {
        [SerializeField] private SavesRuntime savesRuntime;
        public Calendar Calendar => GameSave.calendar;

        public List<ClassificationData> Classifications => GameSave.classificationsData;

        public List<CompetitorData> Competitors => GameSave.competitors;

        /// <summary>Number of jumpers that compete per team in the current event (2 for Duos, 4 otherwise).</summary>
        public int CurrentTeamSize
        {
            get
            {
                var currentEvent = GetCurrentEvent();
                return currentEvent != null && currentEvent.eventType == EventType.Duos
                    ? EventType.Duos.GetSubRoundsCount()
                    : EventType.Team.GetSubRoundsCount();
            }
        }

        public List<TeamData> Teams => GameSave.teams.Where(data => data.competitors.Count >= CurrentTeamSize).ToList();

        public GameSave GameSave => savesRuntime.GetCurrentSave();

        public TeamData EditedTeam { get; set; }
        public EventInfo SelectedEventCalendar { get; set; }

        public string GetRank(RankType rankType, int id)
        {
            //   Debug.Log($"TournamentMenuData. rankType: {rankType} id {id}");
            if (rankType == RankType.Classification)
                return Calendar.classifications[id].name;
            if (rankType == RankType.Event)
                return $"{id + 1}. {Calendar.events[id].hillId}";
            return "";
        }

        public IEnumerable<EventInfo> GetEvents()
        {
            return Calendar.events;
        }

        public void ChangeClassificationPriority(ClassificationData item, int value)
        {
            if (0 > value || value >= GameSave.classificationsData.Count) return;
            var buf = GameSave.classificationsData[value];
            GameSave.classificationsData[value] = item;
            GameSave.classificationsData[item.priority] = buf;
            buf.priority = item.priority;
            item.priority = value;
        }

        public void ChangeCompetitorPriority(CompetitorData item, int value)
        {
            if (0 > value || value >= EditedTeam.competitors.Count) return;
            var buf = EditedTeam.competitors[value];
            EditedTeam.competitors[value] = item;
            EditedTeam.competitors[item.teamId] = buf;
            buf.teamId = item.teamId;
            item.teamId = value;
        }

        public EventInfo GetCurrentEvent()
        {
            var currentId = GameSave.resultsContainer.eventIndex;

            if (currentId >= 0 && GameSave.calendar.events.Count > currentId)
                return GameSave.calendar.events[currentId];
            return null;
        }

        public int GetCurrentEventId()
        {
            return GameSave.resultsContainer.eventIndex;
        }

        public void ChooseBestSquad()
        {
            var squadSize = CurrentTeamSize;

            var bestCompetitors = EditedTeam.competitors
                .OrderByDescending(x => x.competitor.largeHillSkill)
                .Take(squadSize)
                .OrderBy(x => x.competitor.largeHillSkill)
                .ToList();

            var bestIds = new HashSet<int>(
                bestCompetitors.Select(x => x.calendarId));

            var remainingCompetitors = EditedTeam.competitors
                .Where(x => !bestIds.Contains(x.calendarId))
                .ToList();

            EditedTeam.competitors = bestCompetitors
                .Concat(remainingCompetitors)
                .ToList();

            for (var i = 0; i < EditedTeam.competitors.Count; i++)
                EditedTeam.competitors[i].teamId = i;
        }
        public void ChooseBestSquads()
        {
            var squadSize = CurrentTeamSize;

            foreach (var team in Teams)
            {
                var bestCompetitors = team.competitors
                    .OrderByDescending(x => x.competitor.largeHillSkill)
                    .Take(squadSize)
                    .OrderBy(x => x.competitor.largeHillSkill)
                    .ToList();

                var bestIds = new HashSet<int>(
                    bestCompetitors.Select(x => x.calendarId));

                var remainingCompetitors = team.competitors
                    .Where(x => !bestIds.Contains(x.calendarId))
                    .ToList();

                team.competitors = bestCompetitors
                    .Concat(remainingCompetitors)
                    .ToList();

                for (var i = 0; i < team.competitors.Count; i++)
                    team.competitors[i].teamId = i;
            }
        }

    }
}


