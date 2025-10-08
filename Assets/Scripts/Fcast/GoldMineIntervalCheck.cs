using System;
namespace Fcast
{    public class GoldMineIntervalCheck : ExecCheck
    {
        public int Miners { get; set; }
        private bool[] _queue { get; set; }
        private int _qDex { get; set; }
        private DateTime _time { get; set; }
        private TimeSpan _interval { get; set; }

        public override void Exec()
        {
            var now = DateTime.UtcNow;
            var firstExec = _time == default;
            if (firstExec)
                _time = now;
            if (firstExec)
                return;

            for (int i=0; i<5; i++)
            {
                bool mining = Miners >= (i+1);
                _queue[i] = mining;
            }

            var elapsed = now - _time;
            var didElapse = elapsed > _interval;

            if (didElapse)
            {
                Check = _queue[_qDex];
                _qDex = (_qDex + 1) % 5;
                _time = now;
            }
            else
                Check = false;
        }
        public GoldMineIntervalCheck()
        {
            _interval = TimeSpan.FromSeconds(1f); // (max-speed) 5 miners at a rate of 1 unit mined per second
            Miners = 0;
            _queue = new bool[5] { false, false, false, false, false };
            _qDex = 0;
        }
    }
}
