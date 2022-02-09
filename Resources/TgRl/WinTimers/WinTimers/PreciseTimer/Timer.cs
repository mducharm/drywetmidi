using System;
using Common;

namespace PreciseTimer
{
    internal sealed class Timer : ITimer
    {
        private PreciseTimer _timer;

        public void Start(int intervalMs, Action callback)
        {
            _timer = new PreciseTimer(callback, intervalMs);
        }

        public void Stop()
        {
            _timer.Dispose();
        }
    }
}
