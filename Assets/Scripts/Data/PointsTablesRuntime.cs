using System.Collections.Generic;
using OpenSkiJumping.Competition.Persistent;
using UnityEngine;

namespace OpenSkiJumping.Data
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Data/PointsTablesRuntime")]
    public class PointsTablesRuntime : DatabaseObject<List<List<PointsTable>>>
    {
        /// <summary>Returns the tables of the given group, or an empty list when the group does not exist in the data file.</summary>
        public IEnumerable<PointsTable> GetData(int tableIndex) =>
            tableIndex < Data.Count ? Data[tableIndex] : new List<PointsTable>();

        public bool Remove(int tableIndex, PointsTable item)
        {
            return Data[tableIndex].Remove(item);
        }

        public void Add(int tableIndex, PointsTable item)
        {
            Data[tableIndex].Add(item);
        }
    }
}