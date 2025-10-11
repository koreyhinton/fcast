using System;
using System.Collections.Generic;
using UnityEngine;
namespace Fcast
{
    public class XY
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
    public class BuildingUpdateViewsCheck : ExecCheck
    {
        public BuildingEventIntervalCheck BuildingEventIntervalCheck { get; set; }
        public int AddQueryX { get; set; } = -1;
        public int AddQueryY { get; set; } = -1;
        private List<XY> _queries { get; set; } = new List<XY>();

        public override void Exec()
        {
            if (AddQueryX + AddQueryY != -2)
            {
                var newXY = new XY();
                newXY.X = AddQueryX;
                newXY.Y = AddQueryY;
                _queries.Add(newXY);
                AddQueryX = -1;
                AddQueryY = -1;
                Check = true;
                return;
            }
            Debug.Log("executing queries" + _queries.Count);
            bool updated = false;
            foreach (var xy in _queries)
            {
                BuildingEventIntervalCheck.EventType = BuildingEventIntervalType.Query;
                BuildingEventIntervalCheck.X = xy.X;
                BuildingEventIntervalCheck.Y = xy.Y;
                BuildingEventIntervalCheck.Exec();
                if (BuildingEventIntervalCheck)
                {
                    Debug.Log("EVENT TYPE OCURRED: " + BuildingEventIntervalCheck.EventType);
                    switch (BuildingEventIntervalCheck.EventType)
                    {
                        case BuildingEventIntervalType.Construct:
                        {
                            Debug.Log("BUILDING CONSTRUCTED!!!!!!!!!!!!!!1");
                            var prefab = Resources.Load<GameObject>("Assets/Resources/Prefabs/BuildingView.prefab");
                            UnityEngine.Object.Instantiate(
                                prefab,
                                new Vector3((float)xy.X, (float)xy.Y, 0f),
                                Quaternion.identity,
                                /*parent:*/ null
                            );
                            updated = true;
                            break;
                        }
                        default:
                            break;
                    }
                }
            }
            Check = updated;
        }
        public BuildingUpdateViewsCheck()
        {
        }
    }
}
