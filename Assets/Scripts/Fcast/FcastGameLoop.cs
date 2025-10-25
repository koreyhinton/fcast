// START OF LITERATE PROGRAM FILE
// Wik-mode can be used in emacs to expand/collapse sections*.
//     Alt-Shift-* (collapse all)
//     Alt-Shift-o (toggle current section open/closed)
//     Alt-Shift-n (next section header)
//     Alt-Shift-p (previous section header)
//     Alt-Shift-RIGHT_ARROW (preview file path with path marker prefix**)
//     Alt-Shift-LEFT_ARROW (close preview file path with path marker prefix**)
//     Alt-Shift-DOWN_ARROW (open file path for editing in new buffer frame)
//     Alt-Shift-UP_ARROW (close newly opened buffer frame)
// * sections start with ALL-CAPS comment (which is the section header line)
// ** path marker prefix: ./ or ../

using System.Collections.Generic; using System.Linq; using UnityEngine; namespace Fcast { public static class FcastGameLoop { private static bool _once = false; private static int _i = 0; public static void It(FcastGameData g) {

    // START OF GAME LOOP ITERATION - VARIABLE RESETS
    // called from ../../Osnowa/Osnowa.Core/TurnManager.cs
    // var resets
    var angles = new System.Collections.Generic.List<float>();
    angles.Add(-25f);
    angles.Add(25f);
    Dictionary<char, int> costTable = new Dictionary<char, int>()
    {
        {'g', 100}, //goldmine
        {'m', 10},  //(gold)miner
        {'t', 200}, //temple
        {'p', 50}   //priestess
    };
    var player = g.Mages.FirstOrDefault();
    GameObject playerView = null;
    bool playerLoaded = false;
    bool playerBounce = false;
    // bool aimBuildKeyAccepted = false; // final build placement (not preview)
    bool aimKeyPressed = false;
    bool aimWillConstructBuilding = false;
    bool aimWillTryBakeUnit = false;
    int buildingX = -1;
    int buildingY = -1;
    int playerX = -1;
    int playerY = -1;
    bool addBuildingQuery = false;
    bool placingBuildPreview = false;
    bool placingValidBuildPreview = false;

    // ONE-TIME ONLY LOGIC
    if (g.MageResources == null)
    {
        g.MageResources = new Dictionary<ResourceType, Resource>();
        var gold = new Resource() { Type = ResourceType.Gold };
        var ore = new Resource() { Type = ResourceType.Ore };
        var timber = new Resource() { Type = ResourceType.Timber };
        g.MageResources.Add(ResourceType.Gold, gold);
        g.MageResources.Add(ResourceType.Ore, ore);
        g.MageResources.Add(ResourceType.Timber, timber);
    }
    if (!_once)
    {
        g.MageResources[ResourceType.Gold].Amount = 800;
        _once = true;
    }

    // WAIT-FOR-LOAD SET VARS
    if (player != null)
        playerView = ((Osnowa.Osnowa.Unity.EntityViewBehaviour)player.view.Controller).gameObject;
    if (playerView != null)
        playerLoaded = true;
    if (playerLoaded)
    {
        playerX = (int)playerView.transform.position.x; //(int)(playerView.Position.X);
        playerY = (int)playerView.transform.position.y; //(int)(playerView.Position.Y);
    }

    // TODO: SECOND PLAYER LOGIC
    // if (g.Type == GameType.Rtt)
    //     2nd player RTT logic
    // else
    //     1 player RTS-only game

    // RESOURCE COLLECTION INTERVAL CHECKS
    if (playerLoaded)
    {
        var location1 = new TimberCheckLocation() { X = 0, Y = 0 };
        var location2 = new TimberCheckLocation() { X = 1, Y = 0 };
        var location3 = new TimberCheckLocation() { X = 2, Y = 0 };
        g.TimberChopIntervalCheck.ChopperLocations = new List<TimberCheckLocation>() { location1, location2, location3 };
        g.TimberChopIntervalCheck.TreeLocations = new List<TimberCheckLocation>() { location1, location2 };

        g.TimberChopIntervalCheck.Exec();
        g.GoldMineIntervalCheck.Exec();
    }
    if (playerLoaded && g.GoldMineIntervalCheck)
    {
        g.MageResources[ResourceType.Gold].Amount += (1 * g.GoldMineIntervalCheck.Miners);
        // Debug.Log("Gold: " + g.MageResources[ResourceType.Gold].Amount);
    }
    if (playerLoaded && g.TimberChopIntervalCheck)
    {
        g.MageResources[ResourceType.Timber].Amount += g.TimberChopIntervalCheck.ChopOutput;
        // Debug.Log("trees: " + g.MageResources[ResourceType.Timber].Amount);
    }

    // KEYBOARD INPUT SEQUENCE CHECK
    if (playerLoaded)
    {
        g.InputBuildSequenceCheck.Exec();
    }
    if (
        playerLoaded && g.InputBuildSequenceCheck
    )
    {
        aimKeyPressed = true;
    }

    // PLAYER ANIMATION INTERVAL
    if (!g.Over && playerLoaded)
    {
        g.DiffLog.Action = LogAction.Write;
        g.DiffLog.Key = "playerXY";
        g.DiffLog.Value = "(" + playerX + "," + playerY + ")";
        g.DiffLog.Exec();
        g.EventIntervalCheck.Type = EventIntervalCheckType.PlayerBounce;
        g.EventIntervalCheck.Exec();

        g.InputBuildSequenceCheck.Frame = g.Frame;
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

    // GET POSITION FOR BUILD PREVIEW OR REGULAR BUILD
    if (aimKeyPressed)
    {
        buildingX = playerX + g.InputBuildSequenceCheck.Offset.X;
        buildingY = playerY + g.InputBuildSequenceCheck.Offset.Y;
    }

    if (
        aimKeyPressed &&
        g.InputBuildSequenceCheck.PendingBuildingChoice != (char)0
    )
    {
        placingBuildPreview = true;
        Debug.Log("Placing Build preview: "+buildingX+","+buildingY);
    }

    // SHOW BUILDING BLUEPRINT, STOP FURTHER BUILD PROPAGATION
    // buildings (or character units) go from stages:
    //    preview -> spawning -> razing (or fully built and receives damage)
    if (
        placingBuildPreview &&
        g.Grid.IsWalkable(
            new Osnowa.Osnowa.Core.Position(buildingX, buildingY)) &&
        ExecCheck.Imply( // !a || b
            g.InputBuildSequenceCheck.PendingBuildingChoice == 't',
            g.Grid.IsWalkable(
                new Osnowa.Osnowa.Core.Position(buildingX+1, buildingY)) &&
            g.Grid.IsWalkable(
                new Osnowa.Osnowa.Core.Position(buildingX-1, buildingY))
        )
    )
    {
        placingValidBuildPreview = true; // only the temple spans 3 wide
    }

    if (
        g.BuildingPreviewUpdater.PreviewExists &&
        g.InputBuildSequenceCheck.PendingBuildingChoice == (char)0
    )
    {
        // Remove the preview
        Debug.Log("REMOVING THE PREVIEW");
        g.BuildingPreviewUpdater.BuildingChoice
            = g.InputBuildSequenceCheck.PendingBuildingChoice;
        g.BuildingPreviewUpdater.Exec();
    }

    if (placingBuildPreview)
    {
        g.BuildingPreviewUpdater.BuildingChoice
            = g.InputBuildSequenceCheck.PendingBuildingChoice;
        g.BuildingPreviewUpdater.ValidPlacement = placingValidBuildPreview;
        g.BuildingPreviewUpdater.X = buildingX;
        g.BuildingPreviewUpdater.Y = buildingY;
        g.BuildingPreviewUpdater.Exec();
    }
    if (!placingBuildPreview && g.BuildingPreviewUpdater.PreviewExists)
    {
        g.BuildingPreviewUpdater.Exec();
    }

    // AIM AND BUILD
    if (
        aimKeyPressed && /*!placingBuildPreview*/ g.InputBuildSequenceCheck.BuildingChoice != (char)0 && (
        costTable[g.InputBuildSequenceCheck.BuildingChoice]
        < g.MageResources[ResourceType.Gold].Amount) && (
        g.Grid.IsWalkable(new Osnowa.Osnowa.Core.Position(buildingX, buildingY)))
    )
    {
        // enough gold and walkable

        // queue a building spawner
        g.QueuedSpawner = new BuildingSpawner();
        g.QueuedSpawner.X = g.BuildingPreviewUpdater.X;
        g.QueuedSpawner.Y = g.BuildingPreviewUpdater.Y;
        g.QueuedSpawner.BuildingChoice = g.InputBuildSequenceCheck.BuildingChoice;
        g.MageResources[ResourceType.Gold].Amount -= costTable[g.QueuedSpawner.BuildingChoice];
        ((IExec)g.QueuedSpawner).Exec();
    }

    if (g.BuildingSpawner != null)
    {
        g.BuildingSpawner.Exec();
    }
    if (g.BuildingSpawner != null && g.BuildingSpawner)
    {
        g.QueuedRazer = new BuildingRazer();
        g.QueuedRazer.X = g.BuildingSpawner.X;
        g.QueuedRazer.Y = g.BuildingSpawner.Y;
        g.QueuedRazer.BuildingChoice = g.BuildingSpawner.BuildingChoice;
        g.BuildingSpawner = null;
        Debug.Log("NULLed out the spawner");
        ((IExec)g.QueuedRazer).Exec(); // don't care about razed status when created
    }
    if (g.BuildingRazer != null)
    {
        g.BuildingRazer.Exec();
    }
    if (g.BuildingRazer != null && g.BuildingRazer)
    {
        g.BuildingRazer = null;
    }

    // TODO: REMOVE CODE
    /********************
    if (aimBuildKeyAccepted && g.InputBuildSequenceCheck.BuildingChoice != 'p' && g.InputBuildSequenceCheck.BuildingChoice != 'm' && g.InputBuildSequenceCheck.BuildingChoice != 'g')
    {
        // Debug.Log("building " + g.InputBuildSequenceCheck.BuildingChoice + " at: " + buildingX + "," + buildingY);
        g.BuildingEventIntervalCheck.X = buildingX;
        g.BuildingEventIntervalCheck.Y = buildingY;
        g.BuildingEventIntervalCheck.BuildingType = g.InputBuildSequenceCheck.BuildingChoice;
        g.BuildingEventIntervalCheck.EventType = BuildingEventIntervalType.Construct;
        g.BuildingEventIntervalCheck.Seconds = 0;
        g.BuildingEventIntervalCheck.Exec();
    }
    if (aimBuildKeyAccepted && g.InputBuildSequenceCheck.BuildingChoice != 'p' && g.InputBuildSequenceCheck.BuildingChoice != 'm' && g.InputBuildSequenceCheck.BuildingChoice != 'g' && g.BuildingEventIntervalCheck)
    {
        aimWillConstructBuilding = true;
        g.BuildingUpdateViewsCheck.AddQueryX = buildingX;
        g.BuildingUpdateViewsCheck.AddQueryY = buildingY;
        addBuildingQuery = true;
    }

    if (aimBuildKeyAccepted && !aimWillConstructBuilding && g.InputBuildSequenceCheck.BuildingChoice == 'g')
        g.MageResources[ResourceType.Gold].Amount -= costTable['g'];
    if (aimBuildKeyAccepted && !aimWillConstructBuilding && g.InputBuildSequenceCheck.BuildingChoice == 'm')
        g.MageResources[ResourceType.Gold].Amount -= costTable['m'];
    if (aimBuildKeyAccepted && !aimWillConstructBuilding && (
            g.InputBuildSequenceCheck.BuildingChoice == 'g' || (g.BuildingUpdateViewsCheck.Counts['g'] > 0 && g.InputBuildSequenceCheck.BuildingChoice == 'm')))
    {
        g.GoldMineIntervalCheck.Miners = g.BuildingUpdateViewsCheck.Counts['m'];
        aimWillTryBakeUnit = true;
    }

    if (aimBuildKeyAccepted && !aimWillConstructBuilding && g.BuildingUpdateViewsCheck.Counts['t'] > 0 && g.InputBuildSequenceCheck.BuildingChoice == 'p')
    {
        // priestess (only build if there's a temple)
        g.MageResources[ResourceType.Gold].Amount -= costTable['p'];
        aimWillTryBakeUnit = true;
    }
    if (aimWillTryBakeUnit)
    {
        // Debug.Log("Prepare to bake");
        g.BuildingEventIntervalCheck.X = buildingX;
        g.BuildingEventIntervalCheck.Y = buildingY;
        g.BuildingEventIntervalCheck.Seconds = 0;
        g.BuildingEventIntervalCheck.BuildingType = g.InputBuildSequenceCheck.BuildingChoice;
        g.BuildingEventIntervalCheck.EventType = BuildingEventIntervalType.Construct;
        g.BuildingEventIntervalCheck.Exec(); // once for construct
        addBuildingQuery = true;
    }
    ********************/

    // todo: once bake timer bug is fixed, uncommnet to have it be timed:
    /*if (aimWillTryBakeUnit && g.BuildingEventIntervalCheck)
    {
        Debug.Log("Prepare to bake 2");
        addBuildingQuery = true;
        g.BuildingEventIntervalCheck.X = buildingX;
        g.BuildingEventIntervalCheck.Y = buildingY;
        g.BuildingEventIntervalCheck.BuildingType = 'p';
        g.BuildingEventIntervalCheck.EventType = BuildingEventIntervalType.Bake;
        g.BuildingEventIntervalCheck.Seconds = 20; // 20 seconds is max time, 5 is min time
        for (int i=0; i<3 && i<g.BuildingUpdateViewsCheck.Counts['t']; i++) g.BuildingEventIntervalCheck.Seconds /= 2;
        g.BuildingEventIntervalCheck.Exec(); // second for bake
    }*/

    // TODO: REMOVE BUILDING VIEW UPDATES
    /******************8
    if (g.BuildingUpdateViewsCheck.BuildingEventIntervalCheck == null)
    {
        g.BuildingUpdateViewsCheck.BuildingEventIntervalCheck = g.BuildingEventIntervalCheck;
    }
    if (addBuildingQuery)
    {
        g.DiffLog.Action = LogAction.Write;
        g.DiffLog.Key = "buildingchoice_xy";
        g.DiffLog.Value = g.InputBuildSequenceCheck.BuildingChoice + "_("+buildingX+","+buildingY+")";
        g.DiffLog.Exec();
        g.BuildingUpdateViewsCheck.AddQueryX = buildingX;
        g.BuildingUpdateViewsCheck.AddQueryY = buildingY;
    }
    if (playerLoaded)
    {
        g.BuildingUpdateViewsCheck.Exec();
    }
    if (playerLoaded && g.BuildingUpdateViewsCheck)
    {
        g.TempleCount = g.BuildingUpdateViewsCheck.Counts['t'];
        // Debug.Log("Building Views updated");
    }
    ******************/

    // END OF LOOP ITERATION - FLUSH LOG
    {
        g.DiffLog.Action = LogAction.Flush;
        g.DiffLog.Exec();
    }


}}}
