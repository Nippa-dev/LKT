using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace LKtunnel
{
    public partial class WireGuard : UserControl, IConfigurableProtocol
    {
        private Process wireguardProcess;
        private MainWindow mainWindow;
        private string configPath = @"C:\path\to\your\config.conf";
        private const string WireGuardPath = @"C:\Program Files\WireGuard\wireguard.exe";

        public WireGuard()
        {
            InitializeComponent();
            mainWindow = Application.Current.MainWindow as MainWindow;
        }

        // Import configuration
        public void ApplyConfig(ProtocolConfig config)
        {
            configPath = config.WireGuardConfigPath;

            if (WireGuardConfigPath != null)
            {
                WireGuardConfigPath.Text = config.WireGuardConfigPath;
            }
        }

        // Export configuration
        public ProtocolConfig ExportConfig()
        {
            return new ProtocolConfig
            {
                Protocol = "WireGuard",
                WireGuardConfigPath = WireGuardConfigPath?.Text ?? configPath
            };
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            configPath = WireGuardConfigPath.Text;

            if (!File.Exists(WireGuardPath))
            {
                ShowError("WireGuard not found at default location!");
                return;
            }

            if (!File.Exists(configPath))
            {
                ShowError("Configuration file not found!");
                return;
            }

            try
            {
                UpdateStatus("Connecting...", Brushes.Orange);
                LogMessage($"Starting WireGuard with config: {configPath}");

                wireguardProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = WireGuardPath,
                        Arguments = $"/installtunnelservice \"{configPath}\"",
                        Verb = "runas",
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };

                wireguardProcess.Start();
                await Task.Delay(1000);

                if (IsWireGuardRunning())
                {
                    UpdateStatus("Connected", Brushes.Green);
                    LogMessage("WireGuard connected successfully");
                }
                else
                {
                    throw new Exception("WireGuard failed to start");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Connection failed: {ex.Message}");
                UpdateStatus("Error", Brushes.Red);
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var uninstallProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = WireGuardPath,
                        Arguments = "/uninstalltunnelservice",
                        Verb = "runas",
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true
                    }
                };

                uninstallProcess.Start();
                uninstallProcess.WaitForExit(5000);

                foreach (var process in Process.GetProcessesByName("wireguard"))
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(1000);
                        }
                    }
                    catch { }
                }

                foreach (var process in Process.GetProcessesByName("wg"))
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(1000);
                        }
                    }
                    catch { }
                }

                UpdateStatus("Disconnected", Brushes.Red);
                LogMessage("WireGuard successfully disconnected");
            }
            catch (Exception ex)
            {
                LogMessage($"Disconnect error: {ex.Message}");
                UpdateStatus("Disconnect Failed", Brushes.Orange);

                try
                {
                    var netshProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = "interface set interface wg0 admin=disable",
                            Verb = "runas",
                            UseShellExecute = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        }
                    };

                    netshProcess.Start();
                    netshProcess.WaitForExit(3000);

                    UpdateStatus("Disconnected (Fallback)", Brushes.Orange);
                    LogMessage("Used fallback method to disconnect");
                }
                catch (Exception fallbackEx)
                {
                    ShowError($"Both disconnect methods failed:\n{ex.Message}\n{fallbackEx.Message}");
                }
            }
        }

        private bool IsWireGuardRunning()
        {
            try
            {
                var processes = Process.GetProcessesByName("wireguard");
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private void BrowseConfigButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "WireGuard Config|*.conf",
                Title = "Select WireGuard Configuration"
            };

            if (dialog.ShowDialog() == true)
            {
                configPath = dialog.FileName;
                WireGuardConfigPath.Text = configPath;
                LogMessage($"Selected config: {configPath}");
            }
        }

        private void UpdateStatus(string status, Brush color)
        {
            Dispatcher.Invoke(() =>
            {
                VpnStatusLabel.Content = status;
                VpnStatusLabel.Foreground = color;
            });
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() => mainWindow?.LogMessage($"[WireGuard] {message}"));
        }

        private void ShowError(string message)
        {
            Dispatcher.Invoke(() =>
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
        }
    }
}
