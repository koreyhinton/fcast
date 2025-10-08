using System.Collections.Generic; using System.Linq; using UnityEngine; namespace Fcast { public static class FcastGameLoop { private static int _i = 0; public static void It(FcastGameData g) {
    var angles = new System.Collections.Generic.List<float>();
    angles.Add(-25f);
    angles.Add(25f);
    // var spriteTransform = g.Player?.GetComponentInChildren<SpriteRenderer>()?.transform;
    var player = g.Mages.FirstOrDefault();
    GameObject playerView = null;

    bool playerLoaded = false;
    bool playerBounce = false;

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
        Debug.Log("Gold: " + g.MageResources[ResourceType.Gold].Amount);
    }

    if (playerLoaded)
    {
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
        Debug.Log("e.z = " + angles[_i]);
        // e.z = angles[_i];
        //spriteTransform.localEulerAngles = new Vector3(0, 0, angles[_i]); //e;
    }

}}}
