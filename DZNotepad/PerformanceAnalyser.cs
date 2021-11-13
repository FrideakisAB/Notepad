using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using LiveCharts;
using LiveCharts.Wpf;
using System.Diagnostics;

namespace DZNotepad
{
    class PerformanceAnalyser : IDisposable
    {
        CartesianChart chart;
        ChartValues<double> cpuUsage;
        ChartValues<double> memoryUsage;
        ChartValues<double> networkUsage;
        Thread updateThread;
        PerformanceCounter cpuCounter;
        PerformanceCounter ramCounter;
        PerformanceCounter ioCounter;
        bool activeThread = true;

        public PerformanceAnalyser(CartesianChart chart, ChartValues<double> cpuUsage, ChartValues<double> memoryUsage, ChartValues<double> networkUsage)
        {
            this.chart = chart;
            this.cpuUsage = cpuUsage;
            this.memoryUsage = memoryUsage;
            this.networkUsage = networkUsage;

            setupCounters();

            updateThread = new Thread(new ThreadStart(updateCounter));
            updateThread.Start();
        }

        private void updateCounter()
        {
            while (activeThread)
            {
                try
                {
                    insertBack(cpuUsage, Math.Round(cpuCounter.NextValue() / Environment.ProcessorCount, 2));
                    insertBack(memoryUsage, Math.Round(ramCounter.NextValue() / 1024 / 1024, 2));
                    insertBack(networkUsage, Math.Round(ioCounter.NextValue() / 1024 / 1024, 2));
                    chart.Update();
                }
                catch (InvalidOperationException)
                {
                    setupCounters();
                }

                Thread.Sleep(500);
            }
        }

        private void setupCounters()
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

            cpuCounter = new PerformanceCounter("Process", "% Processor Time", name, true);
            ramCounter = new PerformanceCounter("Process", "Private Bytes", name, true);
            ioCounter = new PerformanceCounter("Process", "IO Data Bytes/sec", name, true);

            cpuCounter.NextValue();
            ramCounter.NextValue();
            ioCounter.NextValue();
        }

        private void insertBack(ChartValues<double> values, double value)
        {
            for (int i = 0; i < values.Count - 1; i++)
                values[i] = values[i + 1];

            values[values.Count - 1] = value;
        }

        public void Dispose()
        {
            activeThread = false;
        }
    }
}
