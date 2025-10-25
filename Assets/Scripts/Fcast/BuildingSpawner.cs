using System;
using UnityEngine;
namespace Fcast
{
    public class BuildingSpawner : ExecCheck
    {
        private GameObject _buildGO = null;
        private GameObject _toolGO = null;

        public int X { get; set; }
        public int Y { get; set; }
        public char BuildingChoice { get; set; }
        private TimeSpan _interval { get; set; }
        private DateTime _time { get; set; }
        //private bool _done { get; set; } = false;
        private bool _initialized { get; set; } = false;
        private int _lastActiveSwitchSecond { get; set; }
        private bool _elapsed()
        {
            //if (_done)
            //    return false; // avoids repeat Elapsed firings
            var now = DateTime.UtcNow;
            return (now - _time) > _interval;
        }
        public override void Exec()
        {
            var now = DateTime.UtcNow;
            if (!_initialized)
            {
                _initialized = true;
                _interval = TimeSpan.FromSeconds(8);
                _time = now;

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
                var toolPrefab = Resources.Load<GameObject>("Prefabs/ToolView");
                _toolGO = UnityEngine.Object.Instantiate(
                    toolPrefab,
                    new Vector3((float)X, (float)Y, 0f),
                    Quaternion.identity,
                    /*parent:*/ null
                );
            }
            //bool elapsed = _elapsed();
            if (_elapsed()) //&& !_done)
            {
                // remove once, (building razer re-creates it)
                UnityEngine.Object.Destroy(_buildGO);
                _buildGO = null;
                UnityEngine.Object.Destroy(_toolGO);
                _toolGO = null;
            //}
            //if (elapsed || _done)
            //{
                //_done = true;
                Check = true;
                return;
            }

            if (_initialized && _toolGO != null)
            {
                int currentElapsedSecond = (int)((DateTime.UtcNow - _time).TotalSeconds);
                var toggleCondition = // every second change
                    currentElapsedSecond > _lastActiveSwitchSecond;
                if (toggleCondition)
                {
                    _lastActiveSwitchSecond = currentElapsedSecond;
                    _toolGO.SetActive(!_toolGO.activeSelf);
                }
            }

            Check = false;
        }
    }
}
