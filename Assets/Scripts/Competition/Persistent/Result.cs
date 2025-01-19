using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenSkiJumping.Competition.Persistent
{
    [Serializable]
    public class JumpResults
    {
        public List<JumpResult> results = new List<JumpResult>();
    }

    [Serializable]



    public class Result
    {
        [SerializeField]
        private JumpResults[] results;
        [SerializeField]
        private float qualRankPoints;
        [SerializeField]
        private decimal[] totalResults;
        [SerializeField]
        private int[] bibs;
        [SerializeField]
        private int rank;
        [SerializeField]
        private float totalPoints;
        [SerializeField]
        private float distance;
        [SerializeField]
        private float previousRoundDistance;
        [SerializeField]
        private float style;
        [SerializeField]
        private float previousRoundStyle;
        [SerializeField]
        private float actualGate;
        [SerializeField]
        private float previousRoundGate;
        [SerializeField]
        private float wind;
        [SerializeField]
        private float previousRoundWind;
        [SerializeField]
        private int currentCompetitorId;


        public JumpResults[] Results { get => results; set => results = value; }
        public decimal QualRankPoints { get => (decimal)qualRankPoints; set => qualRankPoints = (float)value; }
        public decimal[] TotalResults { get => totalResults; set => totalResults = value; }
        public int[] Bibs { get => bibs; set => bibs = value; }
        public int Rank { get => rank; set => rank = value; }
        public decimal TotalPoints { get => (decimal)totalPoints; set => totalPoints = (float)value; }

        public decimal Distance { get => (decimal)distance; set => distance = (float)value; }

        public decimal PreviousRoundDistance { get => (decimal)previousRoundDistance; set => previousRoundDistance = (float)value; }

        public decimal Style { get => (decimal)style; set => style = (float)value; }

        public decimal PreviousRoundStyle { get => (decimal)previousRoundStyle; set => previousRoundStyle = (float)value; }

        public decimal ActualGate { get => (decimal)actualGate; set => actualGate = (float)value; }

        public decimal PreviousRoundGate { get => (decimal)previousRoundGate; set => previousRoundGate = (float)value; }

        public decimal Wind { get => (decimal)wind; set => wind = (float)value; }
        public decimal PreviousRoundWind { get => (decimal)previousRoundWind; set => previousRoundWind = (float)value; }
        public int CurrentCompetitorId { get => currentCompetitorId; set => currentCompetitorId = value; }


        public Result()
        {
            // Initialize fields with default values
            results = null;
            qualRankPoints = 0f;
            totalResults = null;
            bibs = null;
            rank = 0;
            totalPoints = 0f;
            distance = 0f;
            previousRoundDistance = 0f;
            style = 0f;
            previousRoundStyle = 0f;
            actualGate = 0f;
            previousRoundGate = 0f;
            wind = 0f;
            previousRoundWind = 0f;
            currentCompetitorId = 0;
        }

        public Result(Result other)
        {
            // Copy simple value types directly
            qualRankPoints = other.qualRankPoints;
            totalPoints = other.totalPoints;
            distance = other.distance;
            previousRoundDistance = other.previousRoundDistance;
            style = other.style;
            previousRoundStyle = other.previousRoundStyle;
            actualGate = other.actualGate;
            previousRoundGate = other.previousRoundGate;
            wind = other.wind;
            previousRoundWind = other.wind;
            rank = other.rank;
            currentCompetitorId = other.currentCompetitorId;

            // Copy arrays (deep copy)
            totalResults = other.totalResults != null ? (decimal[])other.totalResults.Clone() : null;
            bibs = other.bibs != null ? (int[])other.bibs.Clone() : null;

            // Copy JumpResults array
            if (other.results != null)
            {
                results = new JumpResults[other.results.Length];
                for (int i = 0; i < other.results.Length; i++)
                {
                    results[i] = new JumpResults
                    {
                        results = new List<JumpResult>(other.results[i].results)
                    };
                }
            }
        }

    }
}