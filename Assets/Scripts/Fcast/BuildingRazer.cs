using System;
using UnityEngine;
namespace Fcast
{
    public class BuildingRazer : ExecCheck
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int HitDamageReceived { get; set; } = 0;
        private GameObject _buildGO = null;
        private bool _initialized { get; set; } = false;
        public char BuildingChoice { get; set; }
        private int _health { get; set; } = 100;

        public BuildingRazer() : base() {}
        public BuildingRazer(GameObject go) : base()
        {
            _buildGO = go;
            _initialized = true;
        }

        public override void Exec()
        {
            if (!_initialized)
            {
                _initialized = true;
                // spawn once
                string prefabName;
                switch (BuildingChoice)
                {
                    case 't':
                    {
                        prefabName = "Prefabs/BuildingView"; // temple
                        break;
                    }
                    case 'p':
                    {
                        prefabName = "Prefabs/PriestessView"; // priestess
                        break;
                    }
                    case 'g':
                    {
                        prefabName = "Prefabs/GoldmineView";
                        break;
                    }
                    case 'm':
                    {
                        prefabName = "Prefabs/MinerView";
                        break;
                    }
                    default:
                        throw new NotImplementedException();
                }
                var prefab = Resources.Load<GameObject>(prefabName);
                _buildGO = UnityEngine.Object.Instantiate(
                    prefab,
                    new Vector3((float)X, (float)Y, 0f),
                    Quaternion.identity,
                    /*parent:*/ null
                );
            }
            _health -= HitDamageReceived;
            if (_health < 0) _health = 0;
            Check = _health == 0; // fully razed once health is 0
            if (Check && _buildGO != null)
            {
                UnityEngine.Object.Destroy(_buildGO);
                _buildGO = null;
            }
        }
    }
}
