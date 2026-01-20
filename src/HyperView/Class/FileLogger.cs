using System.Diagnostics;
using System.Security;
using System.Text;

namespace HyperView.Class
{
    internal class FileLogger
    {
        // Control if saves log to logfile
        public static bool WriteToFile { get; set; } = true;

        // Control if saves log to Windows eventlog
        public static bool WriteToEventLog { get; set; } = true;

        public static bool WriteOnlyErrorsToEventLog { get; set; } = true;

        // Flag to prevent recursive logging
        private static bool _isLogging;

        // Set date format short
        public static string DateFormat { get; set; } = "dd-MM-yyyy";

        // Set date format long
        public static string DateTimeFormat { get; set; } = "dd-MM-yyyy HH:mm:ss";

        // Get logfile path
        public static string GetLogPath(string df)
        {
            return FileManager.LogFilePath + @"\" + Globals.ToolName.FullName + " Log " + df + ".log";
        }

        // Get datetime
        public static string GetDateTime(DateTime datetime)
        {
            return datetime.ToString(DateTimeFormat);
        }

        // Get date
        public static string GetDate(DateTime datetime)
        {
            return datetime.ToString(DateFormat);
        }

        // Set event type
        public enum EventType
        {
            Warning,
            Error,
            Information,
        }

        // Add message
        public static void Message(string logText, EventType type, int id)
        {
            var now = DateTime.Now;
            var date = GetDate(now);
            var dateTime = GetDateTime(now);
            var logPath = GetLogPath(date);

            // Set where to save log message to
            if (WriteToFile)
                AppendMessageToFile(logText, type, dateTime, logPath, id);
            if (!WriteToEventLog)
                return;
            AddMessageToEventLog(logText, type, dateTime, logPath, id);
        }

        /// <summary>
        /// Writes an application startup banner to the log with system information
        /// </summary>
        public static void WriteStartupBanner()
        {
            var now = DateTime.Now;
            var date = GetDate(now);
            var dateTime = GetDateTime(now);
            var logPath = GetLogPath(date);

            try
            {
                var banner = new StringBuilder();
                banner.AppendLine("================================================================================");
                banner.AppendLine($"  {Globals.ToolName.ShortName} - APPLICATION STARTED");
                banner.AppendLine("================================================================================");
                banner.AppendLine($"  Start Time:          {dateTime}");
                banner.AppendLine($"  Application:         {Globals.ToolName.FullName}");
                banner.AppendLine($"  Version:             {Application.ProductVersion}");
                banner.AppendLine($"  User:                {Environment.UserName}");
                banner.AppendLine($"  Domain User:         {Environment.UserDomainName}\\{Environment.UserName}");
                banner.AppendLine($"  Computer Name:       {Environment.MachineName}");
                banner.AppendLine($"  OS Version:          {OSVersionInfo.Name}");
                banner.AppendLine($"  OS Edition:          {OSVersionInfo.Edition}");
                banner.AppendLine($"  OS Build:            {OSVersionInfo.BuildVersion}");
                banner.AppendLine($"  64-bit OS:           {Environment.Is64BitOperatingSystem}");
                banner.AppendLine($"  64-bit Process:      {Environment.Is64BitProcess}");
                banner.AppendLine($"  Running as Admin:    {ApplicationFunctions.IsRunningAsAdmin()}");
                banner.AppendLine($"  .NET Version:        {Environment.Version}");
                banner.AppendLine($"  Working Directory:   {Environment.CurrentDirectory}");
                banner.AppendLine($"  Processor Count:     {Environment.ProcessorCount}");
                banner.AppendLine("================================================================================");

                if (WriteToFile)
                {
                    WriteRawToFile(banner.ToString(), logPath);
                }

                Message($"Application started - Version {Application.ProductVersion}", EventType.Information, 5000);
            }
            catch (Exception ex)
            {
                Message($"Error writing startup banner: {ex.Message}", EventType.Error, 5001);
            }
        }

        /// <summary>
        /// Writes an application shutdown banner to the log
        /// </summary>
        public static void WriteShutdownBanner()
        {
            var now = DateTime.Now;
            var date = GetDate(now);
            var dateTime = GetDateTime(now);
            var logPath = GetLogPath(date);

            try
            {
                Message($"Application shutting down - Version {Application.ProductVersion}", EventType.Information, 5002);

                var banner = new StringBuilder();
                banner.AppendLine("================================================================================");
                banner.AppendLine($"  {Globals.ToolName.ShortName} - APPLICATION STOPPED");
                banner.AppendLine("================================================================================");
                banner.AppendLine($"  Stop Time:           {dateTime}");
                banner.AppendLine($"  User:                {Environment.UserDomainName}\\{Environment.UserName}");
                banner.AppendLine($"  Computer Name:       {Environment.MachineName}");
                banner.AppendLine("================================================================================");
                banner.AppendLine();

                if (WriteToFile)
                {
                    WriteRawToFile(banner.ToString(), logPath);
                }
            }
            catch (Exception ex)
            {
                Message($"Error writing shutdown banner: {ex.Message}", EventType.Error, 5003);
            }
        }

        /// <summary>
        /// Writes raw text directly to the log file without formatting
        /// </summary>
        private static void WriteRawToFile(string text, string path)
        {
            if (_isLogging) return;
            _isLogging = true;

            try
            {
                // Ensure directory exists
                if (!Directory.Exists(FileManager.LogFilePath))
                {
                    Directory.CreateDirectory(FileManager.LogFilePath);
                }

                // Check write access
                if (!HasWriteAccessToDirectory(FileManager.LogFilePath))
                {
                    WriteToFile = false;
                    return;
                }

                // Append raw text to file
                if (!File.Exists(path))
                {
                    using (var writer = File.CreateText(path))
                    {
                        writer.Write(text);
                    }
                }
                else
                {
                    using (var writer = File.AppendText(path))
                    {
                        writer.Write(text);
                    }
                }
            }
            catch (Exception)
            {
                WriteToFile = false;
            }
            finally
            {
                _isLogging = false;
            }
        }

        // Save message to logfile
        private static void AppendMessageToFile(string mess, EventType type, string dtf, string path, int id)
        {
            if (_isLogging) return; // Prevent recursive logging
            _isLogging = true;

            try
            {
                // Check if file exists else create it
                try
                {
                    if (!Directory.Exists(FileManager.LogFilePath))
                    {
                        Directory.CreateDirectory(FileManager.LogFilePath);
                        //Console.WriteLine("Directory to log files created: " + Files.LogFilePath);
                    }
                }
                catch (Exception ex)
                {
                    if (WriteToEventLog)
                    {
                        AddMessageToEventLog($"Error creating log directory, {ex.Message}", EventType.Error, dtf, path, id);
                        AddMessageToEventLog("Writing log file has been disabled.", EventType.Information, dtf, path, id);

                        Console.WriteLine("No write access to log directory. Writing log file has been disabled.");
                    }
                    WriteToFile = false;
                    return;
                }

                // Check if we have write access to the directory
                if (!HasWriteAccessToDirectory(FileManager.LogFilePath))
                {
                    if (WriteToEventLog)
                    {
                        AddMessageToEventLog("No write access to log directory.", EventType.Error, dtf, path, id);
                        AddMessageToEventLog("Writing log file has been disabled.", EventType.Information, dtf, path, id);

                        Console.WriteLine("No write access to log directory. Writing log file has been disabled.");
                    }
                    WriteToFile = false;
                    return;
                }
                var str = type.ToString().Length > 7 ? "\t" : "\t\t";
                if (!File.Exists(path))
                {
                    using (var text = File.CreateText(path))
                        text.WriteLine(
                            $"{dtf} - [EventID {id}] {type}{str}{mess}");
                }
                else
                {
                    using (var streamWriter = File.AppendText(path))
                        streamWriter.WriteLine(
                            $"{dtf} - [EventID {id}] {type}{str}{mess}");
                }
            }
            catch (Exception ex)
            {
                if (!WriteToEventLog)
                    return;
                AddMessageToEventLog($"Error writing to log file, {ex.Message}", EventType.Error, dtf, path, 0);
                AddMessageToEventLog("Writing log file has been disabled.", EventType.Information, dtf, path, 0);
                WriteToFile = false;
            }
            finally
            {
                _isLogging = false;
            }
        }

        private static bool HasWriteAccessToDirectory(string path)
        {
            try
            {
                // Attempt to get a list of security permissions from the directory.
                // This will raise an exception if the path is read-only or do not have access.
                new DirectoryInfo(path).GetAccessControl();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Save message to Windows event log
        private static void AddMessageToEventLog(string mess, EventType type, string dtf, string path, int id)
        {
            if (_isLogging) return; // Prevent recursive logging
            _isLogging = true;

            try
            {
                if (type != EventType.Error && WriteOnlyErrorsToEventLog)
                    return;
                var eventLog = new EventLog("");
                if (!EventLog.SourceExists(Globals.ToolName.FullName))
                    EventLog.CreateEventSource(Globals.ToolName.FullName, "Application");
                eventLog.Source = Globals.ToolName.FullName;
                eventLog.EnableRaisingEvents = true;
                var type1 = EventLogEntryType.Error;
                switch (type)
                {
                    case EventType.Warning:
                        type1 = EventLogEntryType.Warning;
                        break;
                    case EventType.Error:
                        type1 = EventLogEntryType.Error;
                        break;
                    case EventType.Information:
                        type1 = EventLogEntryType.Information;
                        break;
                }
                eventLog.WriteEntry(mess, type1, id);
            }
            catch (SecurityException ex)
            {
                if (WriteToFile)
                {
                    AppendMessageToFile($"Security exception: {ex.Message}", EventType.Error, dtf, path, id);
                    AppendMessageToFile("Run this software as Administrator once to solve the problem.", EventType.Information, dtf, path, id);
                    AppendMessageToFile("Event log entries have been disabled.", EventType.Information, dtf, path, id);
                    WriteToEventLog = false;
                }
            }
            catch (Exception ex)
            {
                if (WriteToFile)
                {
                    AppendMessageToFile(ex.Message, EventType.Error, dtf, path, id);
                    AppendMessageToFile("Event log entries have been disabled.", EventType.Information, dtf, path, id);
                    WriteToEventLog = false;
                }
            }
            finally
            {
                _isLogging = false;
            }
        }
    }
}
