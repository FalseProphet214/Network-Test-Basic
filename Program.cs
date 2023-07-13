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
        static void GenerateReport(List<PingEvent> events, DateTime testStartTime, DateTime testEndTime, string reportName)
        {
            string reportPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + reportName + ".txt";

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

            int timeoutPeriod = 0;
            
            // Allow end user to name the report. This should help with general organization
            Console.Write("Please enter what you wish the report to be named: ");
            string reportName = Console.ReadLine();

            // This corrently does nothing, however I am researching to add a timer to the program so that it will end and generate the report after a specific time the user enters
            Console.Write("\nHow many hours do you want the program to run (Enter 0 to have it run continuously): ");
            string reportDuration = Console.ReadLine();
            Console.Write("You can press any key at any time to stop the report early.");

            Console.Write("\nDo you want to set an extended timeout period for ping (Y or N): ");
            string timeout = Console.ReadLine();

            if (timeout.ToLower() == "y" || timeout.ToLower() == "yes")
            {
                Console.WriteLine("\nHow long in seconds do you want the timeout period to be: ");
                string period = Console.ReadLine();
                timeoutPeriod = int.Parse(period) * 1000;
            }

            // Where the user enters the neccessary IP Address to ping. I have been using 8.8.8.8 for testing
            Console.Write("\nEnter the IP address to ping: ");
            string ipAddress = Console.ReadLine();

            // Creating the command to run the continuous ping
            string command = "ping " + ipAddress + " -t";
            if (timeoutPeriod > 0)
            {
                command = "ping " + ipAddress + " -t -w " + timeoutPeriod;
            }
            

            // letting the user know which IP is being used
            Console.WriteLine("Pinging " + ipAddress + ". Press any key to stop.");

            // Setting this variable for the report to display the start time of the test
            DateTime testStartTime = DateTime.Now;

            // Using these variables to control how long the report runs
            TimeSpan duration;
            DateTime endTime;

            // if the user enters 0 as the time, the report will run indefinitely until the user tells it to stop
            if (reportDuration == "0")
            {
                duration = TimeSpan.MaxValue;
                endTime = DateTime.MaxValue;
            }
            // Otherwise the program will run for however many hours the user inputs
            // The program will still stop if the user hits any key on the keyboard while the command window is active, but this section can allow the command to run "out of the way" so to speak
            else
            {
                duration = TimeSpan.FromHours(Convert.ToDouble(reportDuration));
                endTime = testStartTime.Add(duration);
            }

            // starting the command
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

                    if (DateTime.Now >= endTime)
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
