using System.Linq; using UnityEngine; namespace Fcast { public static class FcastGameLoop { private static int _i = 0; public static void It(FcastGameData g) {
    var angles = new System.Collections.Generic.List<float>();
    angles.Add(-25f);
    angles.Add(25f);
    // var spriteTransform = g.Player?.GetComponentInChildren<SpriteRenderer>()?.transform;
    var player = g.Mages.FirstOrDefault();

    if(!g.Over && g.Tick && player != null /* && spriteTransform != default*/)
    {
    Debug.Log("test: " + (g.Player == null ? "null!" : "found"));
    Debug.Log("test: " + (g.Player?.GetComponentInChildren<SpriteRenderer>()?.transform == null ? "transform null!" : "transform found"));
        Debug.Log($"Tick. {g.Mages.Count},{g.Monsters.Count}");
        _i = (_i+1) % angles.Count;
        // var e = spriteTransform.localEulerAngles;
        player.transform.Rotate(0, 0, angles[_i]);
        Debug.Log("e.z = " + angles[_i]);
        // e.z = angles[_i];
        //spriteTransform.localEulerAngles = new Vector3(0, 0, angles[_i]); //e;
    }
}}}
