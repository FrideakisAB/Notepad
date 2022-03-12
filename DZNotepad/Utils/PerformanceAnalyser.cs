using System;
using LiveCharts;
using LiveCharts.Wpf;
using System.Threading;
using System.Diagnostics;

namespace DZNotepad
{
    class PerformanceAnalyser : IDisposable
    {
        private CartesianChart Chart;

        private ChartValues<double> CPUUsage;
        private ChartValues<double> MemoryUsage;
        private ChartValues<double> NetworkUsage;

        private Thread UpdateThread;
        private bool ActiveThread = true;

        private PerformanceCounter CPUCounter;
        private PerformanceCounter RAMCounter;
        private PerformanceCounter IOCounter;

        public PerformanceAnalyser(CartesianChart chart, ChartValues<double> cpuUsage, ChartValues<double> memoryUsage, ChartValues<double> networkUsage)
        {
            Chart = chart;
            CPUUsage = cpuUsage;
            MemoryUsage = memoryUsage;
            NetworkUsage = networkUsage;

            SetupCounters();

            UpdateThread = new Thread(new ThreadStart(UpdateCounter));
            UpdateThread.Start();
        }

        private void UpdateCounter()
        {
            while (ActiveThread)
            {
                try
                {
                    InsertBack(CPUUsage, Math.Round(CPUCounter.NextValue() / Environment.ProcessorCount, 2));
                    InsertBack(MemoryUsage, Math.Round(RAMCounter.NextValue() / 1024 / 1024, 2));
                    InsertBack(NetworkUsage, Math.Round(IOCounter.NextValue() / 1024 / 1024, 2));
                    Chart.Update();
                }
                catch (InvalidOperationException)
                {
                    SetupCounters();
                }

                Thread.Sleep(500);
            }
        }

        private void SetupCounters()
        {
            Process process = Process.GetCurrentProcess();
            string name = string.Empty;
            foreach (var instance in new PerformanceCounterCategory("Process").GetInstanceNames())
            {
                if (instance.StartsWith(process.ProcessName))
                {
                    using (var processId = new PerformanceCounter("Process", "ID Process", instance, true))
                    {
                        if (process.Id == (int)processId.RawValue)
                        {
                            name = instance;
                            break;
                        }
                    }
                }
            }

            CPUCounter = new PerformanceCounter("Process", "% Processor Time", name, true);
            RAMCounter = new PerformanceCounter("Process", "Private Bytes", name, true);
            IOCounter = new PerformanceCounter("Process", "IO Data Bytes/sec", name, true);

            CPUCounter.NextValue();
            RAMCounter.NextValue();
            IOCounter.NextValue();
        }

        private void InsertBack(ChartValues<double> values, double value)
        {
            for (int i = 0; i < values.Count - 1; i++)
                values[i] = values[i + 1];

            values[values.Count - 1] = value;
        }

        public void Dispose()
        {
            ActiveThread = false;
        }
    }
}
