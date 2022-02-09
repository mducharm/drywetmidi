using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace PreciseTimer
{
    internal sealed class PreciseTimer : IDisposable
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        static extern int NtQueryTimerResolution(out int MinimumResolution, out int MaximumResolution, out int CurrentResolution);
        [DllImport("ntdll.dll", SetLastError = true)]
        static extern int NtSetTimerResolution(int DesiredResolution, bool SetResolution, out int CurrentResolution);
        [DllImport("ntdll.dll", SetLastError = true)]
        static unsafe extern int NtDelayExecution(bool alertable, long* delayInterval);

        private static TimeSpan _systemTimerResolution = TimeSpanPrecise(15.6);

        private static void AdjustTimerResolution()
        {
            var queryResult = NtQueryTimerResolution(out var min, out var max, out var current);

            if (queryResult != 0) return;

            _systemTimerResolution = TimeSpan.FromTicks(current);

            if (NtSetTimerResolution(max, true, out _) == 0)
            {
                _systemTimerResolution = TimeSpan.FromTicks(max);
            }
        }

        private static TimeSpan TimeSpanPrecise(double value)
        {
            return TimeSpan.FromTicks((long)Math.Round(value * TimeSpan.TicksPerMillisecond));
        }

        private static long _nextId;
        private static readonly Thread TimerThread;
        private static readonly List<PreciseTimer> Timers = new List<PreciseTimer>(5);
        static PreciseTimer()
        {
            AdjustTimerResolution();
            TimerThread = new Thread(TimerLoop)
            {
                Priority = ThreadPriority.Highest,
                Name = $"{nameof(PreciseTimer)} Thread",
                IsBackground = true
            };
            TimerThread.Start();
        }

        private static void TimerLoop()
        {
            while (true)
            {
                TimerTick();
            }
        }

        private static readonly Func<PreciseTimer, TimeSpan> RemainingSelector = t => t.Remaining;


        private static void TimerTick()
        {
            PreciseTimer nextTimer;
            lock (Timers)
            {
                if (Timers.Count == 0)
                {
                    Thread.Sleep(10);
                    return;
                }

                nextTimer = GetNextTimer(Timers);
            }

            while (!nextTimer._disposed)
            {
                if (nextTimer._sw.Elapsed >= nextTimer._period)
                {
                    nextTimer.Tick();
                    break;
                }

                var remaining = nextTimer.Remaining;

                if (remaining > _systemTimerResolution)
                {
                    SleepPrecise(remaining);
                    continue;
                }

                while (nextTimer.Remaining > TimeSpan.Zero)
                {
                    Thread.SpinWait(1000);
                }

                nextTimer.Tick();
                break;
            }
        }

        private static PreciseTimer GetNextTimer(List<PreciseTimer> timers)
        {
            if (timers.Count == 0) return null;

            var result = timers[0];

            for (int i = 1; i < timers.Count; i++)
            {
                if (timers[i].Remaining < result.Remaining)
                {
                    result = timers[i];
                }
            }

            return result;
        }

        private static unsafe void SleepPrecise(TimeSpan timeToSleep)
        {
            var periods = (int)(timeToSleep.TotalMilliseconds / _systemTimerResolution.TotalMilliseconds);

            if (periods == 0)
                return;

            var ticks = -(_systemTimerResolution.Ticks * periods);
            NtDelayExecution(false, &ticks);
        }

        private void Tick()
        {
            if (_disposed) return;

            _sw.Restart();
            Callback();
        }

        private readonly Action _callback;
        private TimeSpan _period;
        private readonly Stopwatch _sw = Stopwatch.StartNew();
        private bool _disposed;
        public TimeSpan Remaining => _period - _sw.Elapsed;

        public PreciseTimer(Action callback, TimeSpan period)
        {
            _callback = callback;
            _period = period;
            lock (Timers)
            {
                Timers.Add(this);
            }
        }

        public PreciseTimer(Action callback, double period) : this(callback, TimeSpanPrecise(period))
        {
        }

        private void Callback()
        {
            try
            {
                _callback();
            }
            catch
            {
                // ignored
            }
        }

        public void Dispose()
        {
            lock (Timers)
            {
                Timers.Remove(this);
            }
            _disposed = true;
        }

        public void Change(TimeSpan period)
        {
            _period = period;
        }

        public void Change(double period)
        {
            Change(TimeSpanPrecise(period));
        }
    }
}
