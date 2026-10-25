#!/bin/bash
set -e

# START OF LITERATE PROGRAM FILE
# Wik-mode sections: ALL-CAPS comment lines are section headers.
#
# FcastMain is the single per-frame entry point for Fcast game-loop code.
# It lives on the empty "FcastMain" GameObject in ../../Scene.unity, which
# starts inactive and is switched on by ../../Osnowa/Osnowa.Core/GameInitializer.cs
# once the world is generated (same as Osnowa's TurnManager object).
#
# Each frame:
#     1. INPUT    - poll Unity input once
#     2. SIMULATE - run game logic in fixed steps (frame-rate independent)
#     3. PRESENT  - push game state out to GameObjects/transforms
#
# Features migrate here from ./FcastGameLoop.cs (called from
# ../../Osnowa/Osnowa.Core/TurnManager.cs) one at a time.

echo "// Bash-assembled code using github.com/koreyhinton/orc, do not modify the .cs files"
v=${1:-fmn_}
# maps

cat << EOF

using UnityEngine;

namespace Fcast
{
    // Runs before Osnowa's TurnManagerBehaviour (default order 0) each frame.
    [DefaultExecutionOrder(-100)]
    public class FcastMain : MonoBehaviour
    {
        // Simulation step length in seconds; logic runs this many times per
        // second no matter what the render frame rate is.
        private const float StepSeconds = 1f / 30f;

        // Longest frame we try to catch up on, so a long hitch (or a
        // breakpoint) doesn't make the next frame run hundreds of steps.
        private const float MaxFrameSeconds = 0.25f;

        // Seconds between player bounce toggles (was the PlayerBounce
        // interval in ./EventIntervalCheck.cs).
        private const float BounceSeconds = 1.3f;

        private float _accumulatedSeconds;

        // Simulation state
        private float _bounceTimer;
        private bool _bounceUp; // is the player tilted?

        // Scene lookups
        private Entitas.IGroup<GameEntity> _players;

        private void OnEnable()
        {
            // GameInitializer only activates this object once the world exists,
            // so the Entitas context is ready here.
            _players = Contexts.sharedInstance.game.GetGroup(GameMatcher.PlayerControlled);
        }

        private void Update()
        {
            // 1. INPUT - READ ONCE PER FRAME
            // todo: poll Input.* here into a plain input object, so the logic
            // below never calls Input directly and sees each press only once.

            // 2. SIMULATE - FIXED STEPS
            _accumulatedSeconds += Mathf.Min(Time.deltaTime, MaxFrameSeconds);
            while (_accumulatedSeconds >= StepSeconds)
            {
                // Keep it free of transforms/Instantiate; queue requests in state instead.
                // Timers advance by StepSeconds rather than counting frames or reading the clock.

                // PLAYER BOUNCE INTERVAL
                _bounceTimer += StepSeconds;
                if (_bounceTimer >= BounceSeconds)
                {
                    _bounceTimer -= BounceSeconds;
                    _bounceUp = !_bounceUp;
                }

                // todo: clear "pressed this frame" input here so it's
                // consumed by the first step only.

                _accumulatedSeconds -= StepSeconds;
            }

            // 3. PRESENT - SYNC STATE TO THE SCENE
            // Only read simulation state here; set absolute values so
            // applying them again is harmless.

            // PLAYER BOUNCE
            foreach (var player in _players)
            {
                if (!player.hasView)
                    continue;
                var view = (Osnowa.Osnowa.Unity.EntityViewBehaviour)player.view.Controller;
                view.transform.localEulerAngles = new Vector3(0, 0, _bounceUp ? 25f : 0f);
            }
        }
    }
}

EOF
