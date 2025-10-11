using System.Collections.Generic; using System.Linq; using UnityEngine; namespace Fcast { public static class FcastGameLoop { private static int _i = 0; public static void It(FcastGameData g) {
    var angles = new System.Collections.Generic.List<float>();
    angles.Add(-25f);
    angles.Add(25f);
    // var spriteTransform = g.Player?.GetComponentInChildren<SpriteRenderer>()?.transform;
    var player = g.Mages.FirstOrDefault();
    GameObject playerView = null;

    bool playerLoaded = false;
    bool playerBounce = false;
    bool aimEnded = false; // todo: handle more key bindings for building types (for now just using builtin ESC)

    if (g.MageResources == null)
    {
        
        g.MageResources = new Dictionary<ResourceType, Resource>();
        var gold = new Resource() { Type = ResourceType.Gold };
        var ore = new Resource() { Type = ResourceType.Ore };
        var timber = new Resource() { Type = ResourceType.Timber };
        
        g.MageResources.Add(ResourceType.Gold, gold);
        g.MageResources.Add(ResourceType.Ore, ore);
        g.MageResources.Add(ResourceType.Timber, timber);
        /*{
            { ResourceType.Gold, new Resource() { Type = ResourceType.Gold } },
            { ResourceType.Ore, new Resource() { Type = ResourceType.Ore } },
            { ResourceType.Timber, new Resource() { Type = ResourceType.Timber } },
        };*/
    }

    if (player != null)
        playerView = ((Osnowa.Osnowa.Unity.EntityViewBehaviour)player.view.Controller).gameObject;
    if (playerView != null && g.Type == GameType.Rts)
        playerLoaded = true;

    if (g.Type == GameType.Rts)
    {
        g.GoldMineIntervalCheck.Miners = 1;
        g.GoldMineIntervalCheck.Exec();
    }
    if (g.GoldMineIntervalCheck)
    {
        g.MageResources[ResourceType.Gold].Amount += 1;
        // Debug.Log("Gold: " + g.MageResources[ResourceType.Gold].Amount);
    }

    if (g.Type == GameType.Rts)
    {
        var location1 = new TimberCheckLocation() { X = 0, Y = 0 };
        var location2 = new TimberCheckLocation() { X = 1, Y = 0 };
        var location3 = new TimberCheckLocation() { X = 2, Y = 0 };

        g.TimberChopIntervalCheck.ChopperLocations = new List<TimberCheckLocation>() { location1, location2, location3 };
        g.TimberChopIntervalCheck.TreeLocations = new List<TimberCheckLocation>() { location1, location2 };
        g.TimberChopIntervalCheck.Exec();
    }
    if (g.TimberChopIntervalCheck)
    {
        g.MageResources[ResourceType.Timber].Amount += g.TimberChopIntervalCheck.ChopOutput;
        // Debug.Log("trees: " + g.MageResources[ResourceType.Timber].Amount);
    }

    if (playerLoaded)
    {
        g.InputSequenceCheck.Exec();

        g.EventIntervalCheck.Type = EventIntervalCheckType.PlayerBounce;
        g.EventIntervalCheck.Exec();
    }
    if (!g.Over && playerLoaded && g.EventIntervalCheck)
    {
        playerBounce = true;
    }
    if (playerBounce)
    {
        _i = (_i+1) % angles.Count;
        // var e = spriteTransform.localEulerAngles;
        playerView.transform.Rotate(0, 0, angles[_i]);
        // Debug.Log("e.z = " + angles[_i]);
        // e.z = angles[_i];
        //spriteTransform.localEulerAngles = new Vector3(0, 0, angles[_i]); //e;
    }
    if (g.InputSequenceCheck)
    {
        // todo: ideally use the location that you'd aim at, but for now just use player position
        aimEnded = true;
    }

    bool aimWillConstructBuilding = false;
    bool aimWillTryBakeUnit = false;
    int playerX = -1;
    int playerY = -1;
    bool addBuildingQuery = false;
    if (aimEnded)
    {
        playerX = (int)playerView.transform.position.x; //(int)(playerView.Position.X);
        playerY = (int)playerView.transform.position.y; //(int)(playerView.Position.Y);
        g.BuildingEventIntervalCheck.X = playerX;
        g.BuildingEventIntervalCheck.Y = playerY;
        g.BuildingEventIntervalCheck.EventType = BuildingEventIntervalType.Construct;
        g.BuildingEventIntervalCheck.Exec();
    }
    if (aimEnded && g.BuildingEventIntervalCheck)
    {
        aimWillConstructBuilding = true;
        g.BuildingUpdateViewsCheck.AddQueryX = playerX;
        g.BuildingUpdateViewsCheck.AddQueryY = playerY;
        addBuildingQuery = true;
    }

    if (aimEnded && !aimWillConstructBuilding)
    {
        aimWillTryBakeUnit = true;
    }
    if (aimWillTryBakeUnit)
    {
        g.BuildingEventIntervalCheck.Seconds = 20;
        g.BuildingEventIntervalCheck.EventType = BuildingEventIntervalType.Bake;
    }
    if (aimWillTryBakeUnit && g.BuildingEventIntervalCheck)
    {
    }

    // BUILDING VIEW UPDATES
    if (g.BuildingUpdateViewsCheck.BuildingEventIntervalCheck == null)
    {
        g.BuildingUpdateViewsCheck.BuildingEventIntervalCheck = g.BuildingEventIntervalCheck;
    }
    if (addBuildingQuery)
    {
        g.BuildingUpdateViewsCheck.AddQueryX = playerX;
        g.BuildingUpdateViewsCheck.AddQueryY = playerY;
    }
    if (playerLoaded)
    {
        g.BuildingUpdateViewsCheck.Exec();
    }
    if (playerLoaded && g.BuildingUpdateViewsCheck)
    {
        // Debug.Log("Building Views updated");
    }



}}}
