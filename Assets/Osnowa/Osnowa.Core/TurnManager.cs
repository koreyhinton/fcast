namespace Osnowa.Osnowa.Core
{
    using Fcast;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Context;
    using Entitas;
    using Example;
    using GameLogic.GameCore;
    using global::Osnowa.Osnowa.Example.ECS;
    using Debug = UnityEngine.Debug;
    using Osnowa.Grid; //using GameLogic.AI.Navigation;
    using System.Linq;

    public class TurnManager : ITurnManager
    {
	private DateTime _time = DateTime.UtcNow;
	private TimeSpan _interval = TimeSpan.FromSeconds(2);

        private int _lastTurnWhenRemovedDeadActors;
        private int _selectedActorIndex;

        private int _currentActorIndex;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        
        private Queue<GameEntity> _entitiesToHaveTurn = new Queue<GameEntity>(100);

        private readonly IWorldClock _worldClock;
        private GameContext _gameContext;
        private readonly PerInitiativeFeature _perInitiativeFeature;
        private readonly RealTimeFeature _realTimeFeature;
        private IGroup<GameEntity> _energyReadyEntities;
        private IGroup<GameEntity> _entitiesWithEnergy;
        private List<BuildingSpawner> _buildingSpawnList = new List<BuildingSpawner>();
        private List<BuildingRazer> _buildingRazeList = new List<BuildingRazer>();
        private FcastGameData _gameData = new FcastGameData();
        private IGrid _grid; // private INavigator _navigator;

        private IOsnowaContextManager _contextManager;

        public TurnManager(IWorldClock worldClock, 
            PerInitiativeFeature perInitiativeFeature, RealTimeFeature realTimeFeature,
            IOsnowaContextManager contextManager, /*INavigator navigator*/ IGrid grid)
        {
            _worldClock = worldClock;
            _perInitiativeFeature = perInitiativeFeature;
            _realTimeFeature = realTimeFeature;
            _contextManager = contextManager;
            _grid = grid; // _navigator = navigator;
        }

        public void OnGameStart()
        {
            _gameContext = Contexts.sharedInstance.game;
            _energyReadyEntities = _gameContext.GetGroup(GameMatcher.EnergyReady);
            _entitiesWithEnergy = _gameContext.GetGroup(GameMatcher.AllOf(GameMatcher.Energy));

            _stopwatch.Start();
        }

        public void Update()
        {
            _stopwatch.Reset();

            if (_gameData.Mages.Count == 0)
            {
                foreach (var gameEntity in _entitiesWithEnergy)
                {
                    if (!gameEntity.hasView) Debug.Log("null View type");
                    else Debug.Log((gameEntity.view.Controller).GetType().Name);
                    if (gameEntity.isPlayerControlled && gameEntity.hasView)
                    {
                        _gameData.Mages.Add(gameEntity);
                    }
                }
            }
            if (_gameData.Monsters.Count == 0)
            {
                foreach (var gameEntity in _entitiesWithEnergy)
                {
                    if (!gameEntity.isPlayerControlled && gameEntity.hasIntegrity)
                    {
                        _gameData.Monsters.Add(gameEntity);
                    }
                }

            }

            bool tick = false;
            var elapsed = DateTime.UtcNow - _time;
            if (elapsed >= _interval)
            {
                _time = DateTime.UtcNow;
                tick = true;
            }
            if (_gameData.Grid == null)
                _gameData.Grid = (Grid)_grid;
            _gameData.Over = false;
            _gameData.Tick = tick;
            _gameData.Type = Fcast.GameType.FirstPlayerRtsSecondPlayerRtt; // todo: get this based on UI 2-player checkbox instead

            List<BuildingSpawner> spawned = new List<BuildingSpawner>();
            List<BuildingRazer> razed = new List<BuildingRazer>();
            List<int> spawnRemoveIndices = new List<int>();
            List<int> razeRemoveIndices = new List<int>();
            int iIt = 0;
            _gameData.Frame = true;
            do
            {
                _gameData.QueuedSpawner = null;
                _gameData.QueuedRazer = null;
                bool spawnSet = false;
                bool razeSet = false;
                if (iIt < _buildingSpawnList.Count)
                {
                    spawnSet = true;
                    _gameData.BuildingSpawner = _buildingSpawnList[iIt];
                }
                else
                    _gameData.BuildingSpawner = null;
                if (iIt < _buildingRazeList.Count)
                {
                    razeSet = true;
                    _gameData.BuildingRazer = _buildingRazeList[iIt];
                }
                else
                    _gameData.BuildingRazer = null;
                FcastGameLoop.It(_gameData);
                _gameData.Frame = false;
                if (razeSet && _gameData.BuildingRazer == null)
                    razeRemoveIndices.Add(iIt);
                if (spawnSet && _gameData.BuildingSpawner == null)
                    spawnRemoveIndices.Add(iIt);
                if (_gameData.QueuedSpawner != null)
                    spawned.Add(_gameData.QueuedSpawner);
                if (_gameData.QueuedRazer != null)
                    razed.Add(_gameData.QueuedRazer);
                iIt++;
            } while(iIt < Math.Max(_buildingSpawnList.Count, _buildingRazeList.Count));
            if (spawnRemoveIndices.Any())
                Debug.Log(spawnRemoveIndices.First());
            foreach (var i in spawnRemoveIndices.OrderByDescending(x => x)) //desc
                { _buildingSpawnList.RemoveAt(i); Debug.Log("removed spawn"); }
            foreach (var i in razeRemoveIndices.OrderByDescending(x => x)) //desc
                _buildingRazeList.RemoveAt(i);

            _buildingSpawnList.AddRange(spawned);
            _buildingRazeList.AddRange(razed);

            bool needsInput = _gameContext.isWaitingForInput && (_gameContext.playerDecision.Decision == Decision.None);
            if (needsInput)
            {
                //QualitySettings.vSyncCount = 1;
                return;
            }

            _entitiesToHaveTurn.Clear();
            foreach (GameEntity gameEntity in _entitiesWithEnergy)
            {
                if (gameEntity.energy.Energy >= 1f)
                {
                    if (!gameEntity.isEnergyReady)
                    {
                        if (gameEntity.isPlayerControlled)
                        {
                            // for investigation of the bug with multiple pre-turn execution: Debug.Log("Before player turn; energy: " + gameEntity.energy.Energy);
                        }
                        gameEntity.isEnergyReady = true;
                        if (!gameEntity.isPreTurnExecuted)
                        {
                            gameEntity.isExecutePreTurn = true;    
                        }
                        gameEntity.isFinishedTurn = false; 
                        _entitiesToHaveTurn.Enqueue(gameEntity);
                    }
                }
            }
            
            bool someEntitiesWaitForControl = _entitiesToHaveTurn.Count > 0;
            if (someEntitiesWaitForControl)
            {
                //QualitySettings.vSyncCount = 0;
                while (_entitiesToHaveTurn.Count > 0)
                {
                    GameEntity currentEntity = _entitiesToHaveTurn.Dequeue();
                    // todo it crashes here if the entity has been destroyed but still is waiting in the queue
                    _perInitiativeFeature.Execute();

                    bool shouldFinishFrame = _gameContext.isWaitingForInput || _stopwatch.ElapsedMilliseconds > 30;
                    if (shouldFinishFrame)
                    {
                        break;
                    }
                }

                // an entity can be destroyed while having energy and
                // apparently this may cause infinite loop if we don't do the below:
                // if (_energyReadyEntities.count == 0)
                //     _gameContext.isEnergyReadyEntitiesExistUsun = false;
                // todo: does it still happen?
            }
            else
            {
                GiveEnergyToAllEntities();

                _worldClock.HandleSegment();
            }
        }

        private void GiveEnergyToAllEntities()
        {
            foreach (GameEntity entity in _entitiesWithEnergy.GetEntities())
            {
                float newEnergy = entity.energy.Energy + entity.energy.EnergyGainPerSegment;
                entity.ReplaceEnergy(entity.energy.EnergyGainPerSegment, newEnergy);
            }
        }
    }
}
