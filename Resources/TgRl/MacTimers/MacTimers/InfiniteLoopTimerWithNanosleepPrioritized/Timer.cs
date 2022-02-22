using System;
using System.Runtime.InteropServices;
using Common;

namespace InfiniteLoopTimerWithNanosleepPrioritized
{
    internal sealed class Timer : ITimer
    {
        private delegate void TimerCallback();

        [DllImport("InfiniteLoopTimerWithNanosleepPrioritized.dylib", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void StartTimer(int intervalMs, TimerCallback callback, out IntPtr info);

        [DllImport("InfiniteLoopTimerWithNanosleepPrioritized.dylib", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void StopTimer(IntPtr info);

        private IntPtr _timerInfo;
        private Action _callback;
        private TimerCallback _timerCallback;

        public void Start(int intervalMs, Action callback)
        {
            _callback = callback;
            _timerCallback = OnTimerTick;
            StartTimer(intervalMs, _timerCallback, out _timerInfo);
        }

        public void Stop()
        {
            StopTimer(_timerInfo);
        }

        private void OnTimerTick()
        {
            _callback();
        }
    }
}
