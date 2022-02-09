using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;

namespace Common
{
    public static class TimerChecker
    {
        private static readonly TimeSpan MeasurementDuration = TimeSpan.FromMinutes(3);
        private static readonly int[] IntervalsToCheck = { 1, 10, 100 };

        public static void Check(ITimer timer)
        {
            Console.WriteLine("Starting measuring...");
            Console.WriteLine($"OS: {Environment.OSVersion}");
            Console.WriteLine($"CPUs: {Environment.ProcessorCount}");
            Console.WriteLine("--------------------------------");

            foreach (var intervalMs in IntervalsToCheck)
            {
                Console.WriteLine($"Measuring interval of {intervalMs} ms...");
                CheckInterval(timer, intervalMs);
            }

            Console.WriteLine("All done.");
        }

        private static void CheckInterval(ITimer timer, int intervalMs)
        {
            var times = new List<long>((int)Math.Round(MeasurementDuration.TotalMilliseconds) + 1);
            var cpuUsage = new List<int>((int)Math.Round(MeasurementDuration.TotalMilliseconds) + 1);
            
            var stopwatch = new Stopwatch();
            var cpuCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);
            var cpuTimer = new System.Timers.Timer(1000);
            var cpuStep = 0;
            cpuTimer.Elapsed += (_, _) =>
            {
                var value = cpuCounter.NextValue();
                if (cpuStep++ % 2 != 0)
                    cpuUsage.Add((int)Math.Round(value / Environment.ProcessorCount));
            };
            
            Action callback = () => times.Add(stopwatch.ElapsedMilliseconds);

            timer.Start(intervalMs, callback);
            cpuTimer.Start();
            stopwatch.Start();

            Thread.Sleep(MeasurementDuration);

            timer.Stop();
            stopwatch.Stop();
            cpuTimer.Stop();
            
            File.WriteAllLines($"cpu_{intervalMs}.txt", cpuUsage.ToArray().Select(u => u.ToString()));

            var deltas = new List<long>();
            var lastTime = 0L;

            foreach (var time in times.ToArray())
            {
                var delta = time - lastTime;
                deltas.Add(delta);
                lastTime = time;
            }

            File.WriteAllLines($"deltas_{intervalMs}.txt", deltas.Select(d => d.ToString()));
        }
    }
}
