using System;
using System.Runtime.InteropServices;
using Common;

namespace RunLoopTimerPrioritizedWithTolerance
{
    internal sealed class Timer : ITimer
    {
        private delegate void TimerCallback();

        [DllImport("RunLoopTimerPrioritizedWithTolerance.dylib", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void OpenTimerSession(out IntPtr handle);

        [DllImport("RunLoopTimerPrioritizedWithTolerance.dylib", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void StartTimer(int intervalMs, IntPtr sessionHandle, TimerCallback callback, out IntPtr info);

        [DllImport("RunLoopTimerPrioritizedWithTolerance.dylib", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void StopTimer(IntPtr sessionHandle, IntPtr info);

        private readonly IntPtr _sessionHandle;

        private IntPtr _timerInfo;
        private Action _callback;
        private TimerCallback _timerCallback;

        public Timer()
        {
            OpenTimerSession(out _sessionHandle);
        }

        public void Start(int intervalMs, Action callback)
        {
            _callback = callback;
            _timerCallback = OnTimerTick;
            StartTimer(intervalMs, _sessionHandle, _timerCallback, out _timerInfo);
        }

        public void Stop()
        {
            StopTimer(_sessionHandle, _timerInfo);
        }

        private void OnTimerTick()
        {
            _callback();
        }
    }
}
