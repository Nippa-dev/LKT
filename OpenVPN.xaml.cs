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
    public partial class OpenVPN : UserControl, IConfigurableProtocol
    {
        private Process openvpnProcess;
        private Thread logThread;
        private CancellationTokenSource statusCheckToken;
        private MainWindow mainWindow; // Reference to MainWindow
        private string configPath = ""; // Holds path to the .ovpn file

        public OpenVPN()
        {
            InitializeComponent();
            mainWindow = Application.Current.MainWindow as MainWindow;
        }

        // Apply imported config
        public void ApplyConfig(ProtocolConfig config)
        {
            configPath = config.OpenVPNConfigPath;
            if (OpenVPNConfigPath != null)
                OpenVPNConfigPath.Text = config.OpenVPNConfigPath;
        }

        // Export config from current state
        public ProtocolConfig ExportConfig()
        {
            return new ProtocolConfig
            {
                Protocol = "OpenVPN",
                OpenVPNConfigPath = OpenVPNConfigPath?.Text ?? configPath
            };
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string openvpnPath = Path.Combine("OpenVPN\\bin", "openvpn.exe");

            // Validate files
            if (!File.Exists(openvpnPath))
            {
                MessageBox.Show("OpenVPN executable not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            configPath = OpenVPNConfigPath.Text;

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
                        Verb = "runas"
                    }
                };

                try
                {
                    openvpnProcess.Start();
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    if (ex.NativeErrorCode == 1223)
                    {
                        MessageBox.Show("Administrator permissions required to run OpenVPN.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show($"OpenVPN failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }

                logThread = new Thread(() => ReadProcessOutput(openvpnProcess)) { IsBackground = true };
                logThread.Start();

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

                    statusCheckToken?.Cancel();

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
                    UpdateVpnStatus(isConnected ? "Connected" : "Disconnected",
                                    isConnected ? Brushes.Green : Brushes.Red);
                });

                await Task.Delay(5000);
            }
        }

        private bool CheckVpnStatus()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("openvpn"))
                {
                    if (!process.HasExited)
                        return true;
                }
            }
            catch { }

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
                mainWindow?.LogMessage(message);
            });
        }

        private void BrowseConfigButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "OpenVPN Configuration Files (*.ovpn)|*.ovpn|All Files (*.*)|*.*",
                Title = "Select OpenVPN Configuration File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                configPath = openFileDialog.FileName;
                OpenVPNConfigPath.Text = configPath;
                MessageBox.Show($"Selected Configuration File:\n{configPath}", "File Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
