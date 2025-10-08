using System.Linq; using UnityEngine; namespace Fcast { public static class FcastGameLoop { private static int _i = 0; public static void It(FcastGameData g) {
    var angles = new System.Collections.Generic.List<float>();
    angles.Add(-25f);
    angles.Add(25f);
    // var spriteTransform = g.Player?.GetComponentInChildren<SpriteRenderer>()?.transform;
    var player = g.Mages.FirstOrDefault();

    bool playerLoaded = false;
    bool playerBounce = false;

    if (player != null)
        playerLoaded = true;

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
        player.transform.Rotate(0, 0, angles[_i]);
        Debug.Log("e.z = " + angles[_i]);
        // e.z = angles[_i];
        //spriteTransform.localEulerAngles = new Vector3(0, 0, angles[_i]); //e;
    }

}}}
