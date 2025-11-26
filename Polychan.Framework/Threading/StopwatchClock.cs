// Stolen from:
// https://github.com/ppy/osu-framework/blob/master/osu.Framework/Timing/StopwatchClock.cs

using System.Diagnostics;

namespace Polychan.GUI.Framework.Threading
{
    public class StopwatchClock : Stopwatch, IAdjustableClock
    {
        private double seekOffset;

        /// <summary>
        /// Keep track of how much stopwatch time we have used at previous rates.
        /// </summary>
        private double rateChangeUsed;

        /// <summary>
        /// Keep track of the resultant time that was accumulated at previous rates.
        /// </summary>
        private double rateChangeAccumulated;

        public StopwatchClock(bool start = false)
        {
            if (start)
                Start();
        }

        public virtual double CurrentTime => stopwatchCurrentTime + seekOffset;

        /// <summary>
        /// The current time, represented solely by the accumulated <see cref="Stopwatch"/> time.
        /// </summary>
        private double stopwatchCurrentTime => (stopwatchMilliseconds - rateChangeUsed) * rate + rateChangeAccumulated;

        private double stopwatchMilliseconds => (double)ElapsedTicks / Frequency * 1000;

        private double rate = 1;

        public double Rate
        {
            get => rate;
            set
            {
                if (rate == value) return;

                rateChangeAccumulated += (stopwatchMilliseconds - rateChangeUsed) * rate;
                rateChangeUsed = stopwatchMilliseconds;

                rate = value;
            }
        }

        public new void Reset()
        {
            resetAccumulatedRate();
            base.Reset();
        }

        public new void Restart()
        {
            resetAccumulatedRate();
            base.Restart();
        }

        public void ResetSpeedAdjustments() => Rate = 1;

        public virtual bool Seek(double position)
        {
            // Determine the offset that when added to stopwatchCurrentTime; results in the requested time value
            seekOffset = position - stopwatchCurrentTime;
            return true;
        }

        public override string ToString() => $@"{GetType().Name} ({Math.Truncate(CurrentTime)}ms)";

        private void resetAccumulatedRate()
        {
            rateChangeAccumulated = 0;
            rateChangeUsed = 0;
        }
    }
}
