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
    int buildingX = -1;
    int buildingY = -1;
    int playerX = -1;
    int playerY = -1;
    bool previewStampedForBuild = false;
    bool placingBuildPreview = false;
    bool placingValidBuildPreview = false;
    bool clickedToSpawnMonster = false;
    Osnowa.Osnowa.Core.Position clickedToSpawnMonsterGridPosition
        = new Osnowa.Osnowa.Core.Position(0,0);

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

    // COLLISION DETECTED
    if (
        playerLoaded &&
        g.RttRazer != null &&
        playerX == g.RttRazer.X &&
        playerY == g.RttRazer.Y &&
        g.LastFrameCollisionState == 0
    )
    {
        g.LastFrameCollisionState = 1;
    }

    // SETUP RECEIVE DAMAGE
    /*if (
        playerLoaded &&
        !player.hasReceiveDamage
    )
    {
        player.AddReceiveDamage(100, player.id.Id);
    }*/

    // POST-CLICK - COLLISION HANDLING
    if (
        playerLoaded &&
        g.RttPathTarget.X != -1 &&
        g.RttPathTarget.X != 0 &&
        g.RttRazer != null &&
        //g.RttFrame == g.RttFrameMax &&//g.RttFrame < g.RttFrameMax &&
        g.LastFrameCollisionState == 1
    )
    {
        // X direction happens first,
        // Rtt/mouse player gets priority in a collision
        g.LastFrameCollisionState = 2;

        player.integrity.Integrity = player.integrity.Integrity-20 >= 0
            ? player.integrity.Integrity - 20
            : 0;
        player.integrity.MaxIntegrity = 100;
        Debug.Log("Hit! -20 damage");
    }

    // if no longer colliding, reset the state back to 0
    if (
        g.LastFrameCollisionState == 2 && (
        playerX != g.RttRazer.X ||
        playerY != g.RttRazer.Y)
    )
    {
        g.LastFrameCollisionState = 0;
    }

    // second priority collision (player unit moves away from monster unit)
    if (
        g.LastFrameCollisionState == 1 &&
        g.RttRazer != null &&
        g.RttPathTarget.X == -1 &&
        g.RttPathTarget.Y == -1 && (
        playerX != g.RttRazer.X ||
        playerY != g.RttRazer.Y)
    )
    {
        Debug.Log("eliminated monster unit");
        g.RttRazer.HitDamageReceived = 100;
        ((IExec)g.RttRazer).Exec(); // eliminate the
        g.RttRazer = null;          // monster unit
        g.RttUnitCount = 0;
        g.LastFrameCollisionState = 0;
    }

    // POST-CLICK - RTT PLAYER MOVE TO TARGET

    if (
        playerLoaded &&
        g.RttPathTarget.X != -1 &&
        g.RttRazer != null &&
        g.RttFrame < g.RttFrameMax
    )
    {
        g.RttFrame++;
    }
    if (
        playerLoaded &&
        g.RttPathTarget.X != -1 &&
        g.RttRazer != null &&
        g.RttFrame == g.RttFrameMax
    )
    {
        g.RttFrame = 0; // reset
        int xDelta = g.RttPathTarget.X - g.RttRazer.X;
        /*Debug.Log("fromX,toX,delta: " +
            g.RttRazer.X + "," + 
            (g.RttRazer.X+xDelta) + "," + 
            xDelta);*/

        g.RttRazer.X +=
            xDelta == 0 
                ? xDelta 
                : xDelta<0 
                    ? -xDelta/xDelta 
                    : xDelta/xDelta;
        ((IExec)g.RttRazer).Exec();
    }

    if (playerLoaded && g.RttRazer != null && g.RttPathTarget.X == g.RttRazer.X)
        g.RttPathTarget.X = -1;

    if (
        playerLoaded &&
        g.RttRazer != null &&
        g.RttPathTarget.X == -1 &&
        g.RttPathTarget.Y != -1 &&
        g.RttFrame < g.RttFrameMax
    )
    {
        g.RttFrame++;
    }
    if (
        playerLoaded &&
        g.RttPathTarget.X == -1 &&
        g.RttPathTarget.Y != -1 &&
        g.RttRazer != null &&
        g.RttFrame == g.RttFrameMax
    )
    {
        g.RttFrame = 0; // reset
        int yDelta = g.RttPathTarget.Y - g.RttRazer.Y;
        g.RttRazer.Y += yDelta == 0
            ? yDelta
            : yDelta<0
                ? yDelta/-yDelta
                : yDelta/yDelta;
        ((IExec)g.RttRazer).Exec();
    }
    if (
        playerLoaded &&
        g.RttPathTarget.X == -1 &&
        g.RttPathTarget.Y != -1 &&
        g.RttRazer != null &&
        g.RttPathTarget.Y == g.RttRazer.Y
    )
    {
        g.RttPathTarget.Y = -1;
    }

    // MOUSE INPUT CHECK - MOVE RTT/SECOND-PLAYER MONSTER UNIT
    if (
        playerLoaded &&
        g.RttUnitCount > 0 &&
        g.RttRazer != null &&
        Input.GetMouseButtonDown(0)
    )
    {
        var grid = UnityEngine.Object.FindObjectOfType<Grid>();
        clickedToSpawnMonster = true;
        Vector3 worldPosition
            = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPosition = grid.WorldToCell(worldPosition);
        // Debug.Log("RttRazer move detected");
        g.RttPathTarget.X = gridPosition.x;
        g.RttPathTarget.Y = gridPosition.y;
    }

    // MOUSE INPUT CHECK - SPAWN RTT/SECOND-PLAYER MONSTER UNIT
    if (
        playerLoaded &&
        g.RttUnitCount == 0 &&
        g.Frame &&
        Input.GetMouseButtonDown(0)
    )
    {
        g.RttUnitCount++;
        var grid = UnityEngine.Object.FindObjectOfType<Grid>();
        clickedToSpawnMonster = true;
        Vector3 worldPosition
            = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPosition = grid.WorldToCell(worldPosition);
        clickedToSpawnMonsterGridPosition = new Osnowa.Osnowa.Core.Position(
            gridPosition.x,
            gridPosition.y
        );
    }

    if (
        clickedToSpawnMonster &&
        g.Grid.IsWalkable(clickedToSpawnMonsterGridPosition)
    )
    {
        string prefabName = "Prefabs/MonsterView";
        var prefab = Resources.Load<GameObject>(prefabName);
        g.QueuedRttRazer = new MonsterBuildingRazer(UnityEngine.Object.Instantiate(
            prefab,
            new Vector3(clickedToSpawnMonsterGridPosition.x, clickedToSpawnMonsterGridPosition.y, 0f),
            Quaternion.identity,
            /*parent:*/ null
        ));
        g.QueuedRttRazer.X = clickedToSpawnMonsterGridPosition.x;
        g.QueuedRttRazer.Y = clickedToSpawnMonsterGridPosition.y;
    }

    // KEYBOARD INPUT SEQUENCE CHECK
    if (playerLoaded)
    {
        // ./InputBuildSequenceCheck.cs
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
                new Osnowa.Osnowa.Core.Position(buildingX-1, buildingY)) &&
            g.Grid.IsWalkable(
                new Osnowa.Osnowa.Core.Position(buildingX-2, buildingY))
        )
    )
    {
        placingValidBuildPreview = true; // only the temple spans 4 wide
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
        previewStampedForBuild = true;

        // queue a building spawner
        g.QueuedSpawner = new BuildingSpawner();
        g.QueuedSpawner.X = g.BuildingPreviewUpdater.X;
        g.QueuedSpawner.Y = g.BuildingPreviewUpdater.Y;
        g.QueuedSpawner.BuildingChoice = g.InputBuildSequenceCheck.BuildingChoice;
        g.MageResources[ResourceType.Gold].Amount -= costTable[g.QueuedSpawner.BuildingChoice];
        ((IExec)g.QueuedSpawner).Exec();
    }
    if (previewStampedForBuild && g.QueuedSpawner.BuildingChoice == 'g')
    {
        Debug.Log("Added to hashset: " + (g.QueuedSpawner.X+","+g.QueuedSpawner.Y));
        g.GoldMineCoords.Add(g.QueuedSpawner.X+","+g.QueuedSpawner.Y);
    }

    if (g.BuildingSpawner != null)
    {
        g.BuildingSpawner.Exec();
    }
    if (
        g.BuildingSpawner != null && g.BuildingSpawner &&
        g.BuildingSpawner.BuildingChoice == 't' // temple spans width of 4
    )
    {
        var leftPosition =
            new Osnowa.Osnowa.Core.Position(
                g.BuildingSpawner.X-1, g.BuildingSpawner.Y
            );
        var twoLeftPosition =
            new Osnowa.Osnowa.Core.Position(
                g.BuildingSpawner.X-2, g.BuildingSpawner.Y
            );
        var rightPosition =
            new Osnowa.Osnowa.Core.Position(
                g.BuildingSpawner.X+1, g.BuildingSpawner.Y
            );
        g.Grid.ActuallySetWalkability(twoLeftPosition, false);
        g.Grid.ActuallySetWalkability(leftPosition, false);
        g.Grid.ActuallySetWalkability(rightPosition, false);
        Debug.Log("SetWalkability " + leftPosition.x + "," + leftPosition.y);
        Debug.Log("SetWalkability " + rightPosition.x + "," + rightPosition.y);
    }
    if (g.BuildingSpawner != null && g.BuildingSpawner)
    {
        var position = new Osnowa.Osnowa.Core.Position(
            g.BuildingSpawner.X, g.BuildingSpawner.Y
        );
        g.Grid.ActuallySetWalkability(position, false);
        Debug.Log("SetWalkability " + position.x + "," + position.y);
        g.QueuedRazer = new BuildingRazer();
        g.QueuedRazer.X = g.BuildingSpawner.X;
        g.QueuedRazer.Y = g.BuildingSpawner.Y;
        g.QueuedRazer.BuildingChoice = g.BuildingSpawner.BuildingChoice;
        g.BuildingSpawner = null;
        ((IExec)g.QueuedRazer).Exec(); // don't care about razed status when created
    }
    if (g.BuildingRazer != null)
    {
        g.BuildingRazer.Exec();
    }
    if (g.BuildingRazer != null && g.BuildingRazer)
    {
        // 0 health
        g.BuildingRazer = null;
    }
    if (
        g.RttRazer != null &&
        g.RttRazer is MonsterBuildingRazer &&
        ((MonsterBuildingRazer)g.RttRazer).Bounty == 0 &&
        g.GoldMineCoords.Contains(g.RttRazer.X+","+g.RttRazer.Y)
    )
    {
        // each Rtt unit can only steal once
        ((MonsterBuildingRazer)g.RttRazer).Bounty  = 100;
        g.MageResources[ResourceType.Gold].Amount -= 100;
        Debug.Log("$100 stolen from Rts player");
    }

    // END OF LOOP ITERATION - FLUSH LOG
    {
        g.DiffLog.Action = LogAction.Flush;
        g.DiffLog.Exec();
    }


}}}
