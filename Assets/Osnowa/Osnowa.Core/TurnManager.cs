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
        private FcastGameData _gameData = new FcastGameData();

        private IOsnowaContextManager _contextManager;

        public TurnManager(IWorldClock worldClock, 
            PerInitiativeFeature perInitiativeFeature, RealTimeFeature realTimeFeature,
            IOsnowaContextManager contextManager)
        {
            _worldClock = worldClock;
            _perInitiativeFeature = perInitiativeFeature;
            _realTimeFeature = realTimeFeature;
            _contextManager = contextManager;
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
            _gameData.Over = false;
            _gameData.Tick = tick;
            _gameData.Type = Fcast.GameType.FirstPlayerRtsSecondPlayerRtt; // todo: get this based on UI 2-player checkbox instead
            FcastGameLoop.It(_gameData);

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
