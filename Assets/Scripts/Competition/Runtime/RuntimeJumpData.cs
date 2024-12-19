using UnityEngine;

namespace OpenSkiJumping.Competition.Runtime
{
    public interface IJumpData
    {
        decimal Distance { get; set; }

        decimal CPUDistance { get; set; }

        decimal[] JudgesMarks { get; set; }
        int GatesDiff { get; }
        int InitGate { set; }
        int Gate { get; set; }
        int JumperSkill { get; set; }
        decimal Wind { get; set; }
        decimal Speed { get; set; }
    }

    [CreateAssetMenu(menuName = "ScriptableObjects/Competition/RuntimeJumpData")]
    public class RuntimeJumpData : ScriptableObject, IJumpData
    {
        [SerializeField] private decimal speed;
        [SerializeField] private decimal distance;
        [SerializeField] private decimal wind;
        [SerializeField] private decimal[] judgesMarks;
        [SerializeField] private int initGate;
        [SerializeField] private int playerSkill;

        public decimal Distance
        {
            get => distance;
            set => distance = value;
        }

        public decimal[] JudgesMarks
        {
            get => judgesMarks;
            set => judgesMarks = value;
        }

        public int GatesDiff => Gate - InitGate;

        public int InitGate { get; set; }
        public int Gate { get; set; }
        public int JumperSkill { get; set; }
        public decimal CPUDistance { get; set; }
        public decimal Wind
        {
            get => wind;
            set => wind = value;
        }

        public decimal Speed
        {
            get => speed;
            set => speed = value;
        }

        public decimal ActualGate
        {
            get => speed;
            set => speed = value;
        }

        public void ResetValues()
        {
            speed = 0;
            distance = 0;
            wind = 0;
            for (int i = 0; i < judgesMarks.Length; i++)
            {
                judgesMarks[i] = 0;
            }
        }
    }
}