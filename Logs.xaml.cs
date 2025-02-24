using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace LKtunnel
{
    public partial class Logs : UserControl
    {
        private FileSystemWatcher logFileWatcher;
        private string logFilePath = @"C:\path\to\your\log.txt"; // Set your log file path here

        public Logs()
        {
            InitializeComponent();
            StartLogFileWatcher();
            this.Unloaded += Logs_Unloaded;
        }

        // Start monitoring the log file for changes
        private void StartLogFileWatcher()
        {
            if (!File.Exists(logFilePath))
            {
                MessageBox.Show("Log file not found!");
                return;
            }

            logFileWatcher = new FileSystemWatcher(Path.GetDirectoryName(logFilePath), Path.GetFileName(logFilePath))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
            };

            logFileWatcher.Changed += OnLogFileChanged;
            logFileWatcher.EnableRaisingEvents = true;

            // Start by reading the existing contents of the log file
            ReadLogFile();
        }

        // Called when the log file is changed (new content added)
        private void OnLogFileChanged(object sender, FileSystemEventArgs e)
        {
            // This is invoked in a different thread, so we need to update the UI on the main thread
            Dispatcher.Invoke(() =>
            {
                // Append new content to the TextBox
                ReadLogFile();
            });
        }

        // Read the content of the log file and append it to the TextBox
        private void ReadLogFile()
        {
            try
            {
                // Read the last few lines from the log file (if any new lines have been added)
                string[] lines = File.ReadAllLines(logFilePath);
                foreach (var line in lines)
                {
                    LogsTextBox.AppendText(line + Environment.NewLine);
                }

                // Scroll to the end
                LogsTextBox.ScrollToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading log file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Clean up the file watcher when the Logs page is unloaded
        private void Logs_Unloaded(object sender, RoutedEventArgs e)
        {
            logFileWatcher?.Dispose();
        }
    }
}
