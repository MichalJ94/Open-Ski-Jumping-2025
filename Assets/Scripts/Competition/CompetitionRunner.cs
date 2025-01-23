using System.Collections.Generic;
using System.Linq;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.Competition.Runtime;
using OpenSkiJumping.Data;
using OpenSkiJumping.Hills;
using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.Scripts2025;
using OpenSkiJumping.Simulation;
using OpenSkiJumping.TVGraphics.SideResults;
using OpenSkiJumping.UI;
using UnityEngine;
using UnityEngine.Events;


namespace OpenSkiJumping.Competition
{
    public class CompetitionRunner : MonoBehaviour
    {
        [SerializeField] private RuntimeCompetitorsList competitors;
        [SerializeField] private MeshScript hill;
        private IHillInfo _hillInfo;
        [SerializeField] private HillsFactory hillsFactory;
        [SerializeField] private HillsRuntime hillsRepository;
        [SerializeField] private SkiJumperDataController skiJumperDataController;
        [SerializeField] private WindGatePanel windGatePanel;
        [SerializeField] private JumpSimulator compensationsJumpSimulator;
        [SerializeField] private ToBeatLineController toBeatLineController;
        [SerializeField] private RoundResultsController roundResultsController;
        [SerializeField] private RuntimeJumpData jumpData;
        [SerializeField] private GameplayExtension gameplayExtension;
        [SerializeField] private GameObject snowParticles;
        [SerializeField] public bool permitCPUJumps = true;

        public UnityEvent onCompetitionFinish;

        public UnityEvent onCompetitionStart;
        public UnityEvent onJumpFinish;
        public UnityEvent onJumpStart;
        public UnityEvent onNewJumper;
        public UnityEvent onRoundCompleted;
        public UnityEvent onRoundFinish;
        public UnityEvent onRoundStart;
        public UnityEvent onSubroundFinish;
        public UnityEvent onSubroundStart;
        public UnityEvent onWindGateChanged;
        public UnityEvent cpuJumpPerformed;
        [SerializeField] private RuntimeResultsManager resultsManager;
        [SerializeField] private SavesRuntime savesRepository;
        [SerializeField] private MainMenuController menuController;
        private System.Random random = new System.Random();
        private Dictionary<int, Color> _bibColors;
        public int bibColors;
        public int jumperCounter;
        public bool jumperCounterReached;
        public int[] LastRankUsable;
        public bool finishCompetition;

        private void Start()
        {
            OnCompetitionStart();
        }

        public void ResetJumperCounter()
        {
            jumperCounterReached = false;
        }

        public void OnJumpFinish()
        {
            if (resultsManager.JumpFinish())
            {
                jumperCounter++;
                /*  if (jumperCounter == _bibColors.Count)
                  {
                      jumperCounterReached = true;
                      Debug.Log("jumperCounter == _bibColors.Count");
                  }*/
                onJumpFinish.Invoke();
                OnJumpStart();


              //Aktywacja kodu ponizej bez komentarzy sprawia, ze gra siê zatrzymuje przed ostatnim skoczkiem serii

              if (jumperCounter == bibColors-1)
                {
                    Debug.Log("ROUND RESULTS MISSION. onJumpFinish.Invoke(); i jumperCounter == bibColors-1.");
                   // onRoundCompleted.Invoke();
                    //permitCPUJumps = false;
                    return;
                }
                return;
            }
            Debug.Log("ROUND RESULTS MISSION. jumperCounterReached = true;");
            jumperCounterReached = true;
            OnSubroundFinish();
        }

        public void PermitCPUJumpsSetActive()
        {
            permitCPUJumps = true;
        }

        public void PermitCPUJumpsSetInactive()
        {
            if (resultsManager.Value.EventInfo.eventType != EventType.Team && resultsManager.Value.EventInfo.roundInfos.Count < 3)
            {
                permitCPUJumps = false;
            }
            else
            {
                PermitCPUJumpsSetActive();
            }
        }
        public void OnSubroundFinish()
        {
            if (resultsManager.SubroundFinish())
            {
                onSubroundFinish.Invoke();
                Debug.Log("ROUND RESULTS MISSION. onSubroundFinish.Invoke();");
                OnSubroundStart();
                return;
            }
            // W ty miejscu pojawia sie problem!

            OnRoundFinish();
        }

        public void OnRoundFinish()
        {
            if (resultsManager.RoundFinish())
            {
                onRoundFinish.Invoke();
                Debug.Log("ROUND RESULTS MISSION. onRoundFinish.Invoke();");
                //  LastRankUsable = resultsManager.Value.LastRank.ToArray();
                if (resultsManager.Value.EventInfo.eventType != EventType.Team && resultsManager.Value.EventInfo.roundInfos.Count < 3)
                {
                    onRoundCompleted.Invoke();
                    permitCPUJumps = false;
                }

                OnRoundStart();
                return;
            }

            if (resultsManager.Value.EventInfo.eventType != EventType.Team && resultsManager.Value.EventInfo.roundInfos.Count < 3)
            {
                onRoundCompleted.Invoke();
            }
            onRoundFinish.Invoke();
            if (resultsManager.Value.EventInfo.eventType == EventType.Team || resultsManager.Value.EventInfo.roundInfos.Count > 2)
            {
                OnCompetitionFinish();
            }
        }

        public void LetCompetitionFinish()
        {
            if(resultsManager.Value.RoundIndex == resultsManager.Value.EventInfo.roundInfos.Count)
            {
                OnCompetitionFinish();
            }
        }
        public void OnCompetitionFinish()
        {

            UpdateClassifications();
            var save = savesRepository.GetCurrentSave();
            save.resultsContainer.eventIndex++;
            savesRepository.SaveData();
            snowParticles.SetActive(false);
            finishCompetition = true;
            menuController.LoadTournamentMenu();

        }

        public void LoadTournamentMenuWithButton()
        {
            Debug.Log($"LoadTournamentMenuWithButton finishCompetition before = ");
            if (finishCompetition)
            {
                finishCompetition = false;
            }

        }

        private void UpdateClassifications()
        {
            var save = savesRepository.GetCurrentSave();
            var eventId = save.resultsContainer.eventIndex;
            var eventInfo = save.calendar.events[eventId];
            var eventResults = save.resultsContainer.eventResults[eventId];
            resultsManager.Value.UpdateEventResults(eventResults);

            foreach (var it in eventInfo.classifications)
            {
                var classificationInfo = save.calendar.classifications[it];
                var resultsUpdate = resultsManager.Value.GetPoints(classificationInfo);
                var classificationResults = save.resultsContainer.classificationResults[it];
                PointsUtils.UpdateClassificationResults(classificationInfo, classificationResults, resultsUpdate);
            }
        }

        public void OnCompetitionStart()
        {
            var save = savesRepository.GetCurrentSave();
            var eventId = save.resultsContainer.eventIndex;
            var currentEventInfo = save.calendar.events[eventId];

            competitors.competitors = save.competitors.Select(it => it.competitor).ToList();
            competitors.teams = save.teams.Select(it => it.team).ToList();

            var eventParticipants = EventProcessor.EventParticipants(save, eventId);
            save.resultsContainer.eventResults[eventId] = new EventResults {participants = eventParticipants};
            var orderedParticipants = GetOrderedParticipants(eventParticipants, save);

            CalculateBibs(save, orderedParticipants);
            HillSetUp(save, eventId, currentEventInfo);

            resultsManager.Initialize(currentEventInfo, orderedParticipants, _hillInfo);
            SetDefaultJumpData();
            windGatePanel.Initialize(hill.profileData.Value.gates);
            DetermineSnow();
            onCompetitionStart.Invoke();
            OnRoundStart();
            OnSubroundStart();
            OnJumpStart();
        }

        private static List<Participant> GetOrderedParticipants(IEnumerable<Participant> eventParticipants,
            GameSave save)
        {
            var participantsDict = eventParticipants.Select(item => item)
                .ToDictionary(item => item.id, item => item);
            var orderedParticipants = EventProcessor.GetCompetitors(save.calendar, save.resultsContainer)
                .Select(it => participantsDict[it]).ToList();
            return orderedParticipants;
        }

        private void SetDefaultJumpData()
        {
            jumpData.Gate = jumpData.InitGate = 1;
            jumpData.Wind = 0;
        }

        private void CalculateBibs(GameSave save, IEnumerable<Participant> orderedParticipants)
        {
            _bibColors = orderedParticipants.SelectMany(it => it.competitors).ToDictionary(it => it, it => Color.white);
            var orderedClassifications = save.classificationsData.Where(it => it.useBib)
                .Select(it => (it.calendarId, it.priority)).Reverse();
            bibColors = _bibColors.Count;
            foreach (var (it, ind) in orderedClassifications)
            {
                var classificationResults = save.resultsContainer.classificationResults[it];
                var classificationInfo = save.classificationsData[it].classification;
                foreach (var id in classificationResults.totalSortedResults.TakeWhile(jumperId =>
                    classificationResults.rank[jumperId] <= 1))
                {
                    var bibColor =
                        SimpleColorPicker.Hex2Color(save.classificationsData[ind].classification.leaderBibColor);

                    if (classificationInfo.eventType == EventType.Individual)
                    {
                        _bibColors[id] = bibColor;
                    }
                    else
                    {
                        foreach (var competitor in save.teams[id].competitors)
                            _bibColors[competitor.calendarId] = bibColor;
                    }
                }
            }
        }
        private string hillId;
        private float storeKPoint;
        private float storeHS;
        private void HillSetUp(GameSave save, int eventId, EventInfo currentEventInfo)
        {
            hillId = save.calendar.events[eventId].hillId;
            hill.profileData.Value = hillsRepository.GetProfileData(hillId);
            hill.landingAreaSO = hillsFactory.landingAreas[(int) currentEventInfo.hillSurface].Value;
            var track = currentEventInfo.hillSurface == HillSurface.Matting
                ? hill.profileData.Value.inrunData.summerTrack
                : hill.profileData.Value.inrunData.winterTrack;
            hill.inrunTrackSO = hillsFactory.inrunTracks[(int) track].Value;
            hill.GenerateMesh();           
            _hillInfo = hillsRepository.GetHillInfo(hillId);
            var (head, tail, gate) = compensationsJumpSimulator.GetCompensations();
            _hillInfo.SetCompensations(head, tail, gate);
            storeHS = hill.GetHSInCompetition();
            storeKPoint = hill.GetKPointInCompetition();
        }

        public float GetHS()
        {
            return storeHS;
        }
        


        public float GetKPoint()
        {
            return storeKPoint;
        }

        public void DetermineSnow()
        {
            int determiner = random.Next(0,100);
            if(determiner <= gameplayExtension.snowChance)
            {
                snowParticles.SetActive(true);
                Debug.Log("SNOW PARTICLES IS ACTIVE. Determiner: " + determiner);
            }
            else
            {
                snowParticles.SetActive(false);
                Debug.Log("SNOW PARTICLES IS NOT ACTIVE. Determiner: " + determiner);
            }

        }


        public void OnRoundStart()
        {
            resultsManager.RoundInit();
            UpdateToBeat();
            onRoundStart.Invoke();
            OnSubroundStart();
           
        }

        public void OnSubroundStart()
        {
            resultsManager.SubroundInit();
            onSubroundStart.Invoke();
            OnJumpStart();
        }

        public void OnJumpStart()
        {
            var id = resultsManager.Value.GetCurrentJumperId();
            onNewJumper.Invoke();
            /*if (skiJumperDataController.GetControl() == 1)
            {
                cpuJumpPerformed.Invoke();
            }*/
            skiJumperDataController.SetValues(_bibColors[id]);

            // Debug.Log("From HillSetup hillid:" + hillId + " storeHS: " + storeHS);
        }

        public void UpdateToBeat()
        {
            if (resultsManager.Value.StartListIndex == 0 && resultsManager.Value.SubroundIndex == 0)
                jumpData.InitGate = jumpData.Gate;
            toBeatLineController.CompensationPoints =
                (float) (_hillInfo.GetGatePoints(jumpData.GatesDiff) + _hillInfo.GetWindPoints(jumpData.Wind));
            //Debug.Log(" toBeatLineController.CompensationPoints: " + toBeatLineController.CompensationPoints);
            onWindGateChanged.Invoke();
        }
    }
}