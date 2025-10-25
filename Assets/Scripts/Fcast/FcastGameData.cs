using Osnowa.Osnowa.Grid;
using UnityEngine;
using System.Collections.Generic;
namespace Fcast
{
    public enum GameType
    {
        FirstPlayerRtsSecondPlayerRtt,
        SinglePlayerRts
    }
    public enum ResourceType
    {
        Gold,
        Ore,
        Timber
    }
    public class Resource
    {
        public ResourceType Type { get; set; }
        public int Amount { get; set; }
    }
    public class FcastGameData
    {
        public int TempleCount { get; set; } = 0;
        public bool Tick { get; set; }
        public bool Over { get; set; } 
        public GameObject Player { get; set; }
        public HashSet<GameEntity> Mages { get; set; } = new HashSet<GameEntity>();
        public HashSet<GameEntity> Monsters { get; set; } = new HashSet<GameEntity>();
        public EventIntervalCheck EventIntervalCheck { get; set; } = new EventIntervalCheck();
        public GoldMineIntervalCheck GoldMineIntervalCheck { get; set; } = new GoldMineIntervalCheck();
        public TimberChopIntervalCheck TimberChopIntervalCheck { get; set; } = new TimberChopIntervalCheck();
        public GameType Type { get; set; }
        public Dictionary<ResourceType, Resource> MageResources { get; set; }
        public BuildingSpawner BuildingSpawner { get; set; } // input (set null to remove it)
        public BuildingRazer BuildingRazer { get; set; } // input (set null to remove it)
        public BuildingSpawner QueuedSpawner { get; set; } // output (for new spawners)
        public BuildingRazer QueuedRazer { get; set; } // output (for new razers)
        //public BuildingEventIntervalCheck BuildingEventIntervalCheck { get; set; } = new BuildingEventIntervalCheck();
        //public BuildingUpdateViewsCheck BuildingUpdateViewsCheck { get; set; } = new BuildingUpdateViewsCheck();
        public BuildingPreviewUpdater BuildingPreviewUpdater { get; set; } = new BuildingPreviewUpdater();
        public InputBuildSequenceCheck InputBuildSequenceCheck { get; set; } = new InputBuildSequenceCheck();
        public DiffLog DiffLog { get; set; } = new DiffLog();
        public IGrid Grid { get; set; }
        public bool Frame { get; set; }
    }
}
