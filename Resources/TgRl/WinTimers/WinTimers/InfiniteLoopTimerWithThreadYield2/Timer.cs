using System;
using System.Diagnostics;
using System.Threading;
using Common;

namespace InfiniteLoopTimerWithThreadYield2
{
    internal sealed class Timer : ITimer
    {
        private bool _running;

        public void Start(int intervalMs, Action callback)
        {
            var thread = new Thread(() =>
            {
                var lastTime = 0L;
                var stopwatch = new Stopwatch();

                _running = true;
                stopwatch.Start();

                while (_running)
                {
                    if (stopwatch.ElapsedMilliseconds - lastTime >= intervalMs)
                    {
                        callback();
                        lastTime = stopwatch.ElapsedMilliseconds;
                    }

                    Thread.Yield();
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
