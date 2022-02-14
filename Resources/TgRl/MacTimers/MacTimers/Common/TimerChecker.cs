using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Common
{
    public static class TimerChecker
    {
        private static readonly TimeSpan MeasurementDuration = TimeSpan.FromMinutes(3);
        private static readonly TimeSpan CpuMeasurementInterval = TimeSpan.FromMilliseconds(500);
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
            var times = new List<long>((int)Math.Ceiling(MeasurementDuration.TotalMilliseconds));
            var cpuUsage = new List<float>((int)Math.Ceiling(MeasurementDuration.TotalMilliseconds / CpuMeasurementInterval.TotalMilliseconds));
            
            var stopwatch = new Stopwatch();
            
            var cpuTimer = new System.Timers.Timer(CpuMeasurementInterval.TotalMilliseconds);
            var cpuStep = 0;

            var cpuUsageStopwatch = new Stopwatch();
            var startCpuUsage = TimeSpan.Zero;
            var startTime = 0L;

            cpuTimer.Elapsed += (_, _) =>
            {
                if (cpuStep++ % 2 != 0)
                {
                    var endTime = DateTime.UtcNow;
                    var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
                    var cpuUsedMs = (float)(endCpuUsage - startCpuUsage).TotalMilliseconds;
                    var totalMsPassed = cpuUsageStopwatch.ElapsedMilliseconds - startTime;
                    var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
                    cpuUsage.Add(cpuUsageTotal * 100);
                }
                else
                {
                    startTime = cpuUsageStopwatch.ElapsedMilliseconds;
                    startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
                }
            };
            
            Action callback = () => times.Add(stopwatch.ElapsedMilliseconds);

            timer.Start(intervalMs, callback);
            stopwatch.Start();
            cpuTimer.Start();
            cpuUsageStopwatch.Start();

            Thread.Sleep(MeasurementDuration);

            timer.Stop();
            stopwatch.Stop();
            cpuTimer.Stop();
            
            File.WriteAllLines($"cpu_{intervalMs}.txt", cpuUsage.ToArray().Select(u => u.ToString("0.##")));

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
