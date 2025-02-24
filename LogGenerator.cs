using System;
using System.IO;

namespace LKtunnel
{
    public static class LogGenerator
    {
        // Get the log file path inside the application directory
        private static string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

        // Method to write a log entry to the file
        public static void WriteLog(string message)
        {
            try
            {
                // Append the log message with a timestamp to the log file
                string logMessage = $"{DateTime.Now}: {message}{Environment.NewLine}";
                File.AppendAllText(logFilePath, logMessage);
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., file access issues)
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
    }
}
