using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Fcast
{
    public class InputSequenceCheck : ExecCheck
    {
        public char BuildingChoice = (char)0; // t -> temple
        public XY Offset = new XY() { X = 0, Y = 0 };

        private char[] _runningSequence = new char[3] { (char)0, (char)0, (char)0 };
        private char _lastKeyPressed = (char)0;

        private void _resetBuild()
        {
            Offset.X = 0;
            Offset.Y = 0;
            _runningSequence[0] = (char)0;
            _runningSequence[1] = (char)0;
            _runningSequence[2] = (char)0;
            _lastKeyPressed = (char)0;
            BuildingChoice = (char)0;
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
                _lastKeyPressed = key;
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
                    _lastKeyPressed = (char)1;
            }
            var expectBuildingKey = _runningSequence[0] == 'a' &&
                _runningSequence[2] == (char)0;
            if (expectBuildingKey && _lastKeyPressed != 't')//can't build at same position
            {
                var keyDownT = Input.GetKeyDown(KeyCode.T);
                if (keyDownT)
                {
                    var key = 't';
                    _runningSequence[1] = key;
                    _lastKeyPressed = key;
                    // a BUILD must happen, so must return early with Check = true
                    BuildingChoice = key;
                    Check = true;
                    return;
                }
            }
            if (expectBuildingKey)
            {
                char key = (char)0;
                var keyDownP = Input.GetKeyDown(KeyCode.P);
                bool keyDownM = false; bool keyDownG = false;

                if (keyDownP)
                { // priestess
                    key = 'p';
                }
                else if (Input.GetKeyDown(KeyCode.M))
                {
                    keyDownM = true;
                    key = 'm';
                }
                else if (Input.GetKeyDown(KeyCode.G))
                {
                    keyDownG = true;
                    key = 'g';
                }

                if (keyDownP || keyDownM || keyDownG)
                {
                    _runningSequence[1] = key;
                    _lastKeyPressed = key;
                    // a BUILD must happen, so must return early with Check = true
                    BuildingChoice = key;
                    Check = true;
                    return;
                }
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
        public InputSequenceCheck()
        {
        }
    }
}
