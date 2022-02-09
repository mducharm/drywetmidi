using System;
using System.Threading;
using Common;

namespace InfiniteLoopTimerWithSleep
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
            });

            thread.Start();
        }

        public void Stop()
        {
            _running = false;
        }
    }
}