using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace LKtunnel
{
    public partial class OpenVPN : UserControl
    {
        private Process openvpnProcess;
        private Thread logThread;
        private CancellationTokenSource statusCheckToken;
        private MainWindow mainWindow; // Reference to MainWindow
        private string configPath = @"C:\Users\klnip\Downloads\nipun-sg2.vpnjantit-udp-2500.ovpn"; // Default config path

        public OpenVPN()
        {
            InitializeComponent();
            mainWindow = Application.Current.MainWindow as MainWindow; // Get reference to MainWindow
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            // Get the application's base directory where the app is located
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Assuming OpenVPN is installed in "C:\Program Files\OpenVPN\bin\openvpn.exe" or a similar path, 
            // but you can make this path dynamic based on installation or user input
            string openvpnPath = Path.Combine(@"OpenVPN\bin", "openvpn.exe");

            // Check if the OpenVPN executable exists
            if (!File.Exists(openvpnPath))
            {
                MessageBox.Show("OpenVPN executable not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if the configuration file exists
            if (!File.Exists(configPath))
            {
                MessageBox.Show("OpenVPN configuration file not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                LogMessage("Connecting to VPN...");
                UpdateVpnStatus("Connecting...", Brushes.Orange);

                openvpnProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = openvpnPath,
                        Arguments = $"--config \"{configPath}\" --auth-nocache",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas" // Run OpenVPN as Administrator
                    }
                };

                openvpnProcess.Start();

                // Start reading OpenVPN logs
                logThread = new Thread(() => ReadProcessOutput(openvpnProcess));
                logThread.IsBackground = true;
                logThread.Start();

                // Start checking VPN status
                statusCheckToken = new CancellationTokenSource();
                Task.Run(() => CheckVpnStatusPeriodically(statusCheckToken.Token));

                MessageBox.Show("OpenVPN Connecting...", "VPN Status", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start OpenVPN: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (openvpnProcess != null && !openvpnProcess.HasExited)
            {
                try
                {
                    openvpnProcess.Kill();
                    openvpnProcess.Dispose();
                    openvpnProcess = null;

                    LogMessage("VPN Disconnected.");
                    UpdateVpnStatus("Disconnected", Brushes.Red);

                    statusCheckToken?.Cancel(); // Stop VPN status checking

                    MessageBox.Show("OpenVPN Disconnected!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to stop OpenVPN: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No active OpenVPN connection found.", "Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ReadProcessOutput(Process process)
        {
            using (StreamReader output = process.StandardOutput)
            using (StreamReader error = process.StandardError)
            {
                string outputLine;
                while ((outputLine = output.ReadLine()) != null)
                {
                    LogMessage(outputLine);
                }

                string errorLine;
                while ((errorLine = error.ReadLine()) != null)
                {
                    LogMessage("ERROR: " + errorLine);
                }
            }
        }

        private async Task CheckVpnStatusPeriodically(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                bool isConnected = CheckVpnStatus();
                Dispatcher.Invoke(() =>
                {
                    if (isConnected)
                    {
                        UpdateVpnStatus("Connected", Brushes.Green);
                    }
                    else
                    {
                        UpdateVpnStatus("Disconnected", Brushes.Red);
                    }
                });

                await Task.Delay(5000); // Check VPN status every 5 seconds
            }
        }

        private bool CheckVpnStatus()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("openvpn"))
                {
                    if (!process.HasExited)
                    {
                        return true; // OpenVPN process is running
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        private void UpdateVpnStatus(string status, Brush color)
        {
            VpnStatusLabel.Content = status;
            VpnStatusLabel.Foreground = color;
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                mainWindow?.LogMessage(message); // Forward logs to MainWindow
            });
        }

        private void BrowseConfigButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "OpenVPN Configuration Files (*.ovpn)|*.ovpn|All Files (*.*)|*.*",
                Title = "Select OpenVPN Configuration File"
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                configPath = openFileDialog.FileName;
                MessageBox.Show($"Selected Configuration File: {configPath}", "File Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
