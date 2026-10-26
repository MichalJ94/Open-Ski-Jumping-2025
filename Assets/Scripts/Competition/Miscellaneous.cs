using System;
using System.Collections.Generic;

namespace OpenSkiJumping.Competition
{
    [Flags]
    public enum JumpResultState
    {
        None = 0,
        Advanced = 1,
        KoLoser = 2
    }

    [Serializable]
    public class Country
    {
        public string alpha2;
        public string en;
        public string ioc;
    }

    [Serializable]
    public class CountryData
    {
        public List<Country> countryList;
        public List<string> spritesList;
    }

    public enum HillSurface
    {
        Snow,
        Matting
    }

    public enum Gender
    {
        Male,
        Female
    }

    public enum Control
    {
        Player,
        CPU
    }

    public enum LimitType
    {
        None,
        Normal,
        Exact
    }

    public enum RoundType
    {
        Normal,
        KO
    }

    public enum EventType
    {
        Individual,
        Team,
        Duos
    }

    public enum ClassificationType
    {
        Place,
        Points,
        Medal
    }

    public enum RankType
    {
        None,
        Event,
        Classification
    }

    public static class EventTypeExtensions
    {
        /// <summary>Returns true for any group-based event type (Team or Duos).</summary>
        public static bool IsGroupEvent(this EventType eventType) =>
            eventType == EventType.Team || eventType == EventType.Duos;

        /// <summary>Returns true when both event types use the same kind of participants (individual jumpers or teams).</summary>
        public static bool IsSameParticipantKind(this EventType eventType, EventType other) =>
            eventType == other || (eventType.IsGroupEvent() && other.IsGroupEvent());

        /// <summary>Returns the number of sub-rounds (jumpers per participant) for a given event type.</summary>
        public static int GetSubRoundsCount(this EventType eventType) => eventType switch
        {
            EventType.Individual => 1,
            EventType.Duos => 2,
            _ => 4
        };
    }
}