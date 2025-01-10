using System;
using System.Collections.Generic;
using System.Linq;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.Scripts2025;
using UnityEngine;

namespace OpenSkiJumping.Competition.Runtime
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Competition/RuntimeResultsManager")]
    public class RuntimeResultsManager : ScriptableObject
    {
        public IResultsManager Value { get; private set; }

        [SerializeField] private RuntimeJumpData jumpData;
        [SerializeField] private GameplayExtension gameplayExtension;

        public void Initialize(EventInfo eventInfo, List<Participant> orderedParticipants,
            IHillInfo hillInfo)
        {
            Value = new ResultsManager(eventInfo, orderedParticipants, hillInfo);
        }

        public void SubroundInit() => Value.SubroundInit();
        public void RoundInit() => Value.RoundInit();
        public bool JumpFinish() => Value.JumpFinish();
        public bool SubroundFinish() => Value.SubroundFinish();
        public bool RoundFinish() => Value.RoundFinish();
        public void RegisterCPUJump() => Value.RegisterCPUJump(jumpData, gameplayExtension);
        public void RegisterJump() => Value.RegisterJump(jumpData, gameplayExtension);
    }
}