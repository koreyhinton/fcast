using System;
namespace Fcast
{
    // NEW EVENT UPDATE NOTE:
    //     A new event type requires 2 updates
    //     See CHANGE A, B
    public class EventIntervalCheck : ExecCheck
    {
        public EventIntervalCheckType Type { get; set; }
        private DateTime[] _times { get; set; }
        private TimeSpan[] _intervals { get; set; }

        public override void Exec()
        {
            var now = DateTime.UtcNow;
            var firstExec = _times[0] == default;
            if (firstExec)
                for (int i=0; i<_times.Length; i++)
                    _times[i] = now;
            if (firstExec)
                return;
            var elapsed = now - _times[(int)Type];
            Check = elapsed > _intervals[(int)Type];

            if (Check)
                _times[(int)Type] = now;
        }
        public EventIntervalCheck()
        {
            var numTypes = Enum.GetValues(typeof(EventIntervalCheckType)).Length;
            _times = new DateTime[numTypes];
            _intervals = new TimeSpan[numTypes];

            // CHANGE A - ADD NEW HARDCODED SPAN VALUE
            _intervals[(int)EventIntervalCheckType.PlayerBounce] = TimeSpan.FromSeconds(1.3f);
        }
    }
    // CHANGE B - ADD NEW TYPE, MUST BE 0-BASED ARRAY NUMBERS
    public enum EventIntervalCheckType
    {
        PlayerBounce = 0
    }
}
