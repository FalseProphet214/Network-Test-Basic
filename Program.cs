using System;
using System.Diagnostics;

namespace PingReporter
{
    class PingEvent
    {
        public bool IsTimeout { get; set; }
        public DateTime Timestamp { get; set; }
        public string TracertOutput { get; set; }

    }

    class Program
    {
        static void GenerateReport(List<PingEvent> events, DateTime testStartTime, DateTime testEndTime, string ReportName, string tracertOutput)
        {
            string reportPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + ReportName + ".txt";

            using (StreamWriter writer = new StreamWriter(reportPath))
            {
                writer.WriteLine("Ping report");
                writer.WriteLine("Test Start Time: " + testStartTime);
                writer.WriteLine("Test End Time: " + testEndTime);

                writer.WriteLine("\nTracert Output:\n");
                writer.WriteLine(tracertOutput);

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
                            writer.WriteLine("\n\nTimeout occurred at: " + pingEvent.Timestamp);
                            isPreviousTimeout = true;
                            isPreviousResponse = false;
                            timeoutCount++;

                            if (timeoutStart != pingEvent.Timestamp)
                            {
                                timeoutStart = pingEvent.Timestamp;
                            }
                            writer.WriteLine("\nTraceRT: \n" + pingEvent.TracertOutput);
                        }
                    }
                    else
                    {
                        if (!isPreviousResponse)
                        {
                            writer.WriteLine("\nPing response resumed at: " + pingEvent.Timestamp);
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

        static string RunTracertInBackground(string ipAddress)
        {
            string tracertCommand = "tracert " + ipAddress;

            using (Process tracertProcess = new Process())
            {
                tracertProcess.StartInfo.FileName = "cmd.exe";
                tracertProcess.StartInfo.RedirectStandardOutput = true;
                tracertProcess.StartInfo.UseShellExecute = false;
                tracertProcess.StartInfo.CreateNoWindow = true;
                tracertProcess.StartInfo.Arguments = "/C " + tracertCommand;

                tracertProcess.Start();

                string tracertOutput = string.Empty;

                while (!tracertProcess.StandardOutput.EndOfStream)
                {
                    string tracertLine = tracertProcess.StandardOutput.ReadLine();
                    tracertOutput += tracertLine + Environment.NewLine;
                }

                tracertProcess.Close();

                return tracertOutput;
            }
        }

        static void Main()
        {
            List<PingEvent> events = new List<PingEvent>();

            int timeoutPeriod = 0;
            bool backgroundTracertStarted = false;
            string backgroundTracertOutput = string.Empty;

            // Allow end user to name the report. This should help with general organization
            Console.Write("Please enter what you wish the report to be named: ");
            string reportName = Console.ReadLine();

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

            // Where the user enters the IP Address to ping
            // I have been using 8.8.8.8 for testing
            Console.Write("\nEnter the IP address to ping: ");
            string ipAddress = Console.ReadLine();

            // Create the command to run the tracert
            string tracertCommand = "tracert " + ipAddress;
            string tracertOutput = string.Empty;

            // Run the tracert command and print the output
            ProcessStartInfo tracertProcessStartInfo = new ProcessStartInfo("cmd.exe", "/C " + tracertCommand)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process tracertProcess = new Process())
            {
                tracertProcess.StartInfo.FileName = "cmd.exe";
                tracertProcess.StartInfo.RedirectStandardOutput = true;
                tracertProcess.StartInfo.UseShellExecute = false;
                tracertProcess.StartInfo.CreateNoWindow = true;
                tracertProcess.StartInfo.Arguments = "/C " + tracertCommand;

                tracertProcess.Start();

                Console.WriteLine("\nTracert Output:\n");

                while (!tracertProcess.StandardOutput.EndOfStream)
                {
                    string tracertLine = tracertProcess.StandardOutput.ReadLine();
                    Console.WriteLine(tracertLine);
                    tracertOutput += tracertLine + Environment.NewLine; // Append the tracertLine to tracertOutput
                }

                tracertProcess.Close();
            }

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
                        if (!backgroundTracertStarted)
                        {
                            backgroundTracertStarted = true;
                            // Run the tracert in the background
                            Task.Run(() =>
                            {
                                backgroundTracertOutput = RunTracertInBackground(ipAddress);
                            });
                        }

                        PingEvent pingEvent = new PingEvent
                        {
                            IsTimeout = true,
                            Timestamp = DateTime.Now,
                            TracertOutput = backgroundTracertOutput // Save the tracert output to the PingEvent object
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
                        backgroundTracertStarted = false;
                        events.Add(pingEvent);
                    }
                }

                process.Close();
            }

            DateTime testEndTime = DateTime.Now;

            GenerateReport(events, testStartTime, testEndTime, reportName, tracertOutput);

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            // To open the report for immediate review
            Process.Start(new ProcessStartInfo("notepad.exe", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + reportName + ".txt"));
        }
    }
}
