using UnityEngine;
namespace Fcast
{
    public class BuildingPreviewUpdater : IExec
    {
        public bool PreviewExists { get; set; } = false;
        public bool ValidPlacement { get; set; } = false;
        public char BuildingChoice { get; set; } = (char)0;
        public int X { get; set; }
        public int Y { get; set; }
        private GameObject _previewBuildGO = null;
        private XY _lastPreviewXY { get; set; } = new XY() { X = -1, Y = -1 };
        private char _lastBuildingChoice { get; set; } = (char)0;

        private void Color()
        {
            var color = ValidPlacement ? new Color(1f, 1f, 1f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
            foreach(var c in _previewBuildGO.GetComponentsInChildren<SpriteRenderer>())
            {
                c.color = color;
            }
        }

        private void Create()
        {
            string prefabName = string.Empty;
            if (BuildingChoice == 't')
            {
                prefabName = "Prefabs/BuildingView"; // temple
            }
            if (BuildingChoice == 'p')
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
            _previewBuildGO = UnityEngine.Object.Instantiate(
                prefab,
                new Vector3((float)X, (float)Y, 0f),
                Quaternion.identity,
                /*parent:*/ null
            );
            Color();
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
                PreviewExists = false;
            }
            if (removeBuild)
                return;
            if (changeBuild || firstBuild)
            {
                Create();
                _lastBuildingChoice = BuildingChoice;
                _lastPreviewXY.X = X;
                _lastPreviewXY.Y = Y;
                PreviewExists = true;
            }
            else if (X != _lastPreviewXY.X || Y != _lastPreviewXY.Y)
            {
                // just update position
                _previewBuildGO.transform.position = new Vector3(X, Y, 0f);
                Color();
                _lastPreviewXY.X = X;
                _lastPreviewXY.Y = Y;
            }
        }
    }
}
