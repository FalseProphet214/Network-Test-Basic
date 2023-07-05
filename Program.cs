using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace PingReporter
{
    class PingEvent
    {
        public bool IsTimeout { get; set; }
        public DateTime Timestamp { get; set; }
    }

    class Program
    {
        static void GenerateReport(List<PingEvent> events, DateTime testStartTime, DateTime testEndTime, string ReportName)
        {
            string reportPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + ReportName + ".txt";

            using (StreamWriter writer = new StreamWriter(reportPath))
            {
                writer.WriteLine("Ping report");
                writer.WriteLine("Test Start Time: " + testStartTime);
                writer.WriteLine("Test End Time: " + testEndTime);

                writer.WriteLine("\nPing Events:\n");

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
                            writer.WriteLine("Timeout occurred at: " + pingEvent.Timestamp);
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
                            writer.WriteLine("Ping response resumed at: " + pingEvent.Timestamp);
                            isPreviousResponse = true;
                            isPreviousTimeout = false;

                            TimeSpan timeoutPeriod = pingEvent.Timestamp - timeoutStart;

                            if (timeoutPeriod > longestTimeoutPeriod)
                            {
                                longestTimeoutPeriod = timeoutPeriod;
                            }

                            // I genuinly don't remember why I put this here and the code runs without it for now. But I am also paranoid and dont wanna delete it
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


            // Allow end user to name the report. This should help with general organization
            Console.Write("Please enter what you wish the report to be named: ");
            string reportName = Console.ReadLine();

            // This corrently does nothing, however I am researching to add a timer to the program so that it will end and generate the report after a specific time the user enters
            Console.Write("\nHow many hours do you want the program to run (Enter 0 to have it run continuously): ");
            string reportDuration = Console.ReadLine();
            Console.Write("You can press any key at any time to stop the report early.\n \n");

            // Where the user enters the neccessary IP Address to ping. I have been using 8.8.8.8 for testing
            Console.Write("Enter the IP address to ping: ");
            string ipAddress = Console.ReadLine();

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

            GenerateReport(events, testStartTime, testEndTime, reportName);

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
