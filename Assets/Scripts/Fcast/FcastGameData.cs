using UnityEngine;
using System.Collections.Generic;
namespace Fcast
{
    public enum GameType
    {
        Rtt,
        Rts
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
    }
}
