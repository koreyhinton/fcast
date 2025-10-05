using UnityEngine;
using System.Collections.Generic;
namespace Fcast
{
    public class FcastGameData
    {
        public bool Tick { get; set; }
        public bool Over { get; set; } 
        public GameObject Player { get; set; }
        public HashSet<GameObject> Mages { get; set; } = new HashSet<GameObject>();
        public HashSet<GameEntity> Monsters { get; set; } = new HashSet<GameEntity>();
    }
}
