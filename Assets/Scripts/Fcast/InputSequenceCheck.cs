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

        private const int GridSize = 1; // 32
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
                var keyDownUp = Input.GetKeyDown(KeyCode.UpArrow);
                var keyDownDown = Input.GetKeyDown(KeyCode.DownArrow);
                var keyDownLeft = Input.GetKeyDown(KeyCode.LeftArrow);
                var keyDownRight = Input.GetKeyDown(KeyCode.RightArrow);
                if (keyDownDown)
                    Offset.Y -= 1*GridSize;
                if (keyDownUp)
                    Offset.Y += 1*GridSize;
                if (keyDownLeft)
                    Offset.X -= 1*GridSize;
                if (keyDownRight)
                    Offset.X += 1*GridSize;
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
                var keyDownP = Input.GetKeyDown(KeyCode.P);
                if (keyDownP)
                { // priestess
                    var key = 'p';
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
                _runningSequence[1] != (char)0 &&
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
