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
    }
}