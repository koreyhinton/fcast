using UnityEngine;
namespace Fcast
{
    public class BuildingPreviewUpdater : IExec
    {
        public char BuildingChoice { get; set; } = (char)0;
        public int X { get; set; }
        public int Y { get; set; }
        private GameObject _previewBuildGO = null;
        private XY _lastPreviewXY { get; set; } = new XY() { X = -1, Y = -1 };
        private char _lastBuildingChoice { get; set; } = (char)0;

        private void Create()
        {
            string prefabName = string.Empty;
            if (BuildingChoice == 't')
            {
                prefabName = "Prefabs/BuildingView"; // temple
            }
            if (BuildingChoice == 'p') // todo: use the Bake case below instead (once it is working)
            {
                prefabName = "Prefabs/PriestessView"; // priestess
            }
            if (BuildingChoice == 'g')
            {
                prefabName = "Prefabs/GoldmineView";
            }
            if (BuildingChoice == 'm')
            {
                prefabName = "Prefabs/MinerView";
            }
            var prefab = Resources.Load<GameObject>(prefabName);
            UnityEngine.Object.Instantiate(
                prefab,
                new Vector3((float)X, (float)Y, 0f),
                Quaternion.identity,
                /*parent:*/ null
            );
        }

        public void Exec()
        {
            bool firstBuild = _previewBuildGO == null;
            bool removeBuild = BuildingChoice == (char)0 &&
                _previewBuildGO != null;
            bool changeBuild = !removeBuild && _previewBuildGO != null &&
                _lastBuildingChoice != BuildingChoice;
            if (removeBuild || changeBuild)
            {
                UnityEngine.Object.Destroy(_previewBuildGO);
                _previewBuildGO = null;
            }
            if (removeBuild)
                return;
            if (changeBuild || firstBuild)
            {
                Create();
                _lastPreviewXY.X = X;
                _lastPreviewXY.Y = Y;
            }
            else if (X != _lastPreviewXY.X || Y != _lastPreviewXY.Y)
            {
                // just update position
                _previewBuildGO.transform.position = new Vector3(X, Y, 0f);
                _lastPreviewXY.X = X;
                _lastPreviewXY.Y = Y;
            }
        }
    }
}
