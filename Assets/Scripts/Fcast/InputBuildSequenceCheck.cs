using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Fcast
{
    public class InputBuildSequenceCheck : ExecCheck
    {
        public char BuildingChoice = (char)0; // t -> temple
        public char PendingBuildingChoice = (char)0;
        public XY Offset = new XY() { X = 0, Y = 0 };

        // Running Sequence: a => to aim
        //                   build char => to show transparent building*
        //                   same build char again => starts the build
        //  * doesn not affect _runningSequence
        private char[] _runningSequence = new char[3] { (char)0, (char)0, (char)0 };

        // Which key presses will affect _runningSequence and
        // _lastSequenceKeyPressed?
        //
        // The first time pressing a building choice only gets counted as a 
        // sequence key press if PendingBuildingChoice was already set to that 
        // same key. Examples:
        //     (1) by default ['t'] temple appears when aiming
        //     (2) changing from ['t] to ['p'] priestess affects pending 
        //         build choice, you have to hit 'p' again to lock in that unit
        private char _lastSequenceKeyPressed = (char)0;

        private void _resetBuild()
        {
            Offset.X = 0;
            Offset.Y = 0;
            _runningSequence[0] = (char)0;
            _runningSequence[1] = (char)0;
            _runningSequence[2] = (char)0;
            _lastSequenceKeyPressed = (char)0;
            BuildingChoice = (char)0;
            PendingBuildingChoice = (char)0;
        }

        private bool ResolveBuildingKey(char key, KeyCode keyCode)
        {
            // The _lastSequenceKeyPressed != key checks are to make
            // sure you don't build in the same position repeatedly
            // (arrow keys in between will set _lastSequenceKeyPressed to
            // char 1)

            var keyDownCheck = Input.GetKeyDown(keyCode);
            if (keyDownCheck && _lastSequenceKeyPressed != key)
            {
                if (PendingBuildingChoice == key)
                {
                    _runningSequence[1] = key;
                    _lastSequenceKeyPressed = key;
                    // a BUILD must happen, so must return early with Check = true
                    PendingBuildingChoice = (char)0;
                    BuildingChoice = key;
                }
                else
                {
                    PendingBuildingChoice = key;
                }
                return true;
            }
            return false;
        }

        private bool ResolveBuildingKey()
        {
            // t=>temple, p=>priestess, g=>goldmine, m=>(gold)miner
            if (ResolveBuildingKey('t', KeyCode.T))
                return true;
            if (ResolveBuildingKey('p', KeyCode.P))
                return true;
            if (ResolveBuildingKey('m', KeyCode.M))
                return true;
            if (ResolveBuildingKey('g', KeyCode.G))
                return true;
            return false;
        }

        public override void Exec()
        {
            // SEQUENCE STEP 1 - 'A' KEY
            var expectAKey = _runningSequence[1] == (char)0;
            var keyDownA = Input.GetKeyDown(KeyCode.A);
            if (expectAKey && keyDownA)
            {
                var key = 'a';
                _runningSequence[0] = key;
                _lastSequenceKeyPressed = key;
                PendingBuildingChoice = 't';
            }

            // SEQUENCE STEP 2 (OPTIONAL & REPEATABLE) - AIM AND BUILD
            //     LEFT,RIGHT,DOWN,UP
            //     BUILDING KEY, IE: T-KEY
            var expectOffsetKeyDown = _runningSequence[0] == 'a' &&
                _runningSequence[2] == (char)0;
            if (expectOffsetKeyDown)
            {
                bool keyDownUp = false;
                bool keyDownDown = false;
                bool keyDownLeft = false;
                bool keyDownRight = false;
                {
                    bool keypad7 = Input.GetKeyDown(KeyCode.Keypad7);
                    bool keypad9 = Input.GetKeyDown(KeyCode.Keypad9);
                    bool keypad1 = Input.GetKeyDown(KeyCode.Keypad1);
                    bool keypad3 = Input.GetKeyDown(KeyCode.Keypad3);
                    keyDownUp = Input.GetKeyDown(KeyCode.UpArrow)
                        || keypad7
                        || Input.GetKeyDown(KeyCode.Keypad8)
                        || keypad9
                    ;
                    keyDownDown = Input.GetKeyDown(KeyCode.DownArrow)
                        || keypad1
                        || Input.GetKeyDown(KeyCode.Keypad2)
                        || keypad3
                    ;
                    keyDownLeft = Input.GetKeyDown(KeyCode.LeftArrow)
                        || keypad7
                        || Input.GetKeyDown(KeyCode.Keypad4)
                        || keypad1
                    ;
                    keyDownRight = Input.GetKeyDown(KeyCode.RightArrow)
                        || keypad9
                        || Input.GetKeyDown(KeyCode.Keypad6)
                        || keypad3
                    ;
                }
                if (keyDownDown)
                    Offset.Y -= 1; // 1 grid unit movement
                if (keyDownUp)
                    Offset.Y += 1;
                if (keyDownLeft)
                    Offset.X -= 1;
                if (keyDownRight)
                    Offset.X += 1;
                if (keyDownUp || keyDownLeft || keyDownRight || keyDownDown)
                    _lastSequenceKeyPressed = (char)1;
            }
            var expectBuildingKey = _runningSequence[0] == 'a' &&
                _runningSequence[2] == (char)0;

            if (expectBuildingKey && ResolveBuildingKey())
            {
                Check = true;
                return;
            }

            // SEQUENCE STEP 3 - ESC TO CANCEL AIM
            var expectEscKey = _runningSequence[0] == 'a' &&
                //_runningSequence[1] != (char)0 &&
                _runningSequence[2] == (char)0;
            if (expectEscKey)
            {
                var keyDownEsc = Input.GetKeyDown(KeyCode.Escape);
                if (keyDownEsc)
                {
                    _runningSequence[2] = (char)27/*ESC*/;
                    _resetBuild();
                    Check = false;
                    return;
                }
            }
            Check = false;
        }
        public InputBuildSequenceCheck()
        {
        }
    }
}
