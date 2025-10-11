using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Fcast
{
    public class InputSequenceCheck : ExecCheck
    {
        private char[] _runningSequence = new char[2] { (char)0, (char)0 };
        private char[] _buildingSequence = new char[2] { 'a', (char)27 /*ESC*/ };

        public override void Exec()
        {
            if (Input.GetKeyDown(KeyCode.A) && _runningSequence[1] == (char)0)
            {
                _runningSequence[0] = 'a';
            }
            if (Input.GetKeyDown(KeyCode.Escape) && _runningSequence[0] == _buildingSequence[0])
            {
                _runningSequence[0] = (char)0;
                _runningSequence[1] = (char)0;
                Check = true;
                return;
            }
            Check = false;
        }
        public InputSequenceCheck()
        {
        }
    }
}
