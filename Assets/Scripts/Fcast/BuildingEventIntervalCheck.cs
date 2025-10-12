using System;
using System.Collections.Generic;
namespace Fcast
{
    public class BuildingTimer
    {
        public DateTime Time { get; set; }
        public TimeSpan Interval { get; set; }
        public BuildingEventIntervalType EventType { get; set; }
        public bool Done { get; set; } = false;
        public char BuildingType { get; set; } = (char)0;
        public bool Elapsed()
        {
            if (Done)
                return false; // avoids repeat Elapsed firings
            var now = DateTime.UtcNow;
            return (now - Time) > Interval;
        }
    }
    public enum BuildingEventIntervalType
    {
        Construct,
        Bake, // buildings bake up them units
        Raze,
        Query // query for one of the above-specified types
    }
    public class BuildingEventIntervalCheck : ExecCheck
    {
        public char BuildingType { get; set; } = (char)0;
        public float Seconds { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public BuildingEventIntervalType EventType { get; set; }

        public Dictionary<string, BuildingTimer> Timers;

        public override void Exec()
        {
            var now = DateTime.UtcNow;
            var firstExec = Timers == default;
            if (firstExec)
                Timers = new Dictionary<string, BuildingTimer>();

            if (Timers.TryGetValue(X+","+Y, out BuildingTimer timer))
            {
                if (EventType == BuildingEventIntervalType.Construct)
                {
                    Check = false; // cannot build here
                    return;
                }
                if (EventType == BuildingEventIntervalType.Query)
                {
                    BuildingType = timer.BuildingType;
                    EventType = timer.EventType;
                }
                if (EventType == BuildingEventIntervalType.Raze)
                {
                    // Building raze is immediate (invoked by RTS' own player)
                    Timers.Remove(X+","+Y);
                    Check = true; // succeeding in razing the building
                    return;
                }
                if (timer.Elapsed())
                {
                    if (timer.EventType == BuildingEventIntervalType.Construct)
                        timer.Done = true;
                    else
                        timer.Time = now;
                    Check = true;
                    return;
                }
                Check = false;
            }
            else
            {
                if (EventType == BuildingEventIntervalType.Query)
                {
                    Check = false; // nothing to query
                    return;
                }
                if (EventType == BuildingEventIntervalType.Construct)
                {
                    Timers.Add(X+","+Y, new BuildingTimer
                    {
                        Time = now,
                        Interval = TimeSpan.FromSeconds(Seconds),
                        EventType = EventType,
                        BuildingType = BuildingType
                    });
                    Check = true;  // can build here
                    return;
                }
                Check = false;
            }

        }
        public BuildingEventIntervalCheck()
        {
        }
    }
}
