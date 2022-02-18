using System;
using System.Threading;
using Common;

namespace InfiniteLoopTimerWithSleepPrioritized
{
    public sealed class Timer : ITimer
    {
        private bool _running;

        public void Start(int intervalMs, Action callback)
        {
            var thread = new Thread(() =>
            {
                _running = true;

                while (_running)
                {
                    Thread.Sleep(intervalMs);
                    callback();
                }
            })
            { Priority = ThreadPriority.Highest };

            thread.Start();
        }

        public void Stop()
        {
            _running = false;
        }
    }
}