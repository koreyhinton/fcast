using System;
using System.Collections.Generic;
using System.Linq;
namespace Fcast
{
    public class TimberCheckLocation
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
    public class TimberChopIntervalCheck : ExecCheck
    {
        public List<TimberCheckLocation> ChopperLocations { get; set; }
        public List<TimberCheckLocation> TreeLocations { get; set; }
        public int ChopOutput { get; set; }

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

            var elapsed = now - _time;
            var didElapse = elapsed > _interval;

            if (didElapse)
            {
                ChopOutput = 0;
                foreach (var chopper in ChopperLocations)
                {
                    if (TreeLocations.Any(t => t.X == chopper.X && t.Y == chopper.Y))
                        ChopOutput++;
                }

                Check = ChopOutput > 0;
                _time = now;
            }
            else
            {
                Check = false;
                ChopOutput = 0;
            }
        }
        public TimberChopIntervalCheck()
        {
            ChopperLocations = new List<TimberCheckLocation>();
            TreeLocations = new List<TimberCheckLocation>();
            _interval = TimeSpan.FromSeconds(3f); // for now they chop in unison
            ChopOutput = 0;
        }
    }
}
