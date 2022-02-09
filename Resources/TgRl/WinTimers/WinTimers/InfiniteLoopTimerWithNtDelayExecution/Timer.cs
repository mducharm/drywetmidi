using System;
using System.Runtime.InteropServices;
using System.Threading;
using Common;

namespace InfiniteLoopTimerWithNtDelayExecution
{
    internal sealed class Timer : ITimer
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);

        [DllImport("ntdll.dll")]
        private static extern bool NtDelayExecution(bool Alertable, ref long DelayInterval);

        private Thread _thread;
        private bool _running;

        public void Start(int intervalMs, Action callback)
        {
            var res = (uint)(intervalMs * 10000);
            NtSetTimerResolution(res, true, ref res);

            _thread = new Thread(() =>
            {
                _running = true;

                while (_running)
                {
                    var interval = -intervalMs * 10000L;
                    NtDelayExecution(false, ref interval);
                    callback();
                }
            }) { Priority = ThreadPriority.Highest };

            _thread.Start();
        }

        public void Stop()
        {
            _running = false;
        }
    }
}
