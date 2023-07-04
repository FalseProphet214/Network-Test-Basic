using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PingReporter
{
    class PingEvent
    {
        public bool IsTimeout { get; set; }
        public DateTime Timestamp { get; set; }
    }

    class Program
    {
        static void GenerateReport(List<PingEvent> events, DateTime testStartTime, DateTime testEndTime)
        {
            string reportPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\report.txt";

            using (StreamWriter writer = new StreamWriter(reportPath))
            {
                writer.WriteLine("Ping report");
                writer.WriteLine("Test Start Time: " + testStartTime);
                writer.WriteLine("Test End Time: " + testEndTime);

                // writer.WriteLine("\nPing Events:\n");

                bool isPreviousTimeout = false;
                bool isPreviousResponse = false;
                int timeoutCount = 0;
                TimeSpan longestTimeoutPeriod = TimeSpan.Zero;
                DateTime timeoutStart = DateTime.Now;
                DateTime previousEventTime = DateTime.Now;

                foreach (PingEvent pingEvent in events)
                {
                    if (pingEvent.IsTimeout)
                    {
                        if (!isPreviousTimeout)
                        {
                            // writer.WriteLine("Timeout occurred at: " + pingEvent.Timestamp);
                            isPreviousTimeout = true;
                            isPreviousResponse = false;
                            timeoutCount++;

                            if (timeoutStart != pingEvent.Timestamp)
                            {
                                timeoutStart = pingEvent.Timestamp;
                            }
                        }
                    }
                    else
                    {
                        if (!isPreviousResponse)
                        {
                            // writer.WriteLine("Ping response resumed at: " + pingEvent.Timestamp);
                            isPreviousResponse = true;
                            isPreviousTimeout = false;

                            TimeSpan timeoutPeriod = pingEvent.Timestamp - timeoutStart;

                            if (timeoutPeriod > longestTimeoutPeriod)
                            {
                                longestTimeoutPeriod = timeoutPeriod;
                            }

                            // timeoutStart = DateTime.Now;
                        }
                    }

                    previousEventTime = pingEvent.Timestamp;
                }

                if (isPreviousTimeout && previousEventTime != DateTime.MinValue)
                {
                    TimeSpan timeoutPeriod = previousEventTime - timeoutStart;

                    if (timeoutPeriod > longestTimeoutPeriod)
                    {
                        longestTimeoutPeriod = timeoutPeriod;
                    }
                }

                writer.WriteLine("\nTotal Timeouts: " + timeoutCount);
                if (timeoutCount > 0)
                {
                    writer.WriteLine("Longest Timeout Period: " + longestTimeoutPeriod);
                }
            }

            Console.WriteLine("Report generated successfully.");
        }

        static void Main()
        {
            List<PingEvent> events = new List<PingEvent>();

            Console.Write("Enter the IP address to ping: ");
            string ipAddress = Console.ReadLine();
            ipAddress = "192.168.50.154";

            string command = "ping " + ipAddress + " -t";

            Console.WriteLine("Pinging " + ipAddress + ". Press any key to stop.");

            DateTime testStartTime = DateTime.Now;

            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.StartInfo.Arguments = "/C " + command;

                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();

                    Console.WriteLine(line);

                    if (Console.KeyAvailable)
                    {
                        break;
                    }

                    if (line.Contains("Request timed out") || line.Contains("Destination host unreachable"))
                    {
                        PingEvent pingEvent = new PingEvent
                        {
                            IsTimeout = true,
                            Timestamp = DateTime.Now
                        };
                        events.Add(pingEvent);
                    }
                    else if (line.Contains("Reply from"))
                    {
                        PingEvent pingEvent = new PingEvent
                        {
                            IsTimeout = false,
                            Timestamp = DateTime.Now
                        };
                        events.Add(pingEvent);
                    }
                }

                process.Close();
            }

            DateTime testEndTime = DateTime.Now;

            GenerateReport(events, testStartTime, testEndTime);

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
