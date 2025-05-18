using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Microsoft.Win32; // Required for registry editing

namespace LKtunnel
{
    public partial class V2Ray : UserControl, IConfigurableProtocol
    {
        private Process v2rayProcess;
        private MainWindow mainWindow;
        public TextBox MainWindowLogsTextBox { get; set; }

        public V2Ray()
        {
            InitializeComponent();
            mainWindow = Application.Current.MainWindow as MainWindow;
        }

        // Import config (used when loading a .lktconf file)
        public void ApplyConfig(ProtocolConfig config)
        {
            V2RayConfigPath.Text = config.V2RayConfigPath;
        }

        // Export config (used when saving to .lktconf file)
        public ProtocolConfig ExportConfig()
        {
            return new ProtocolConfig
            {
                Protocol = "V2Ray",
                V2RayConfigPath = V2RayConfigPath.Text
            };
        }

        private void BrowseConfig_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                Title = "Select V2Ray Configuration File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                V2RayConfigPath.Text = openFileDialog.FileName;
            }
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                Title = "Save V2Ray Config Path"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, V2RayConfigPath.Text);
                    MessageBox.Show("Configuration path saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadConfig_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                Title = "Load V2Ray Config Path"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    V2RayConfigPath.Text = File.ReadAllText(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void Connect_Click(object sender, RoutedEventArgs e)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string v2rayPath = Path.Combine(appDirectory, @"V2Ray\v2ray.exe");
            string configPath = V2RayConfigPath.Text;

            if (!File.Exists(v2rayPath) || !File.Exists(configPath))
            {
                MessageBox.Show("V2Ray executable or config file missing!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                v2rayProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = v2rayPath,
                        Arguments = $"run -config \"{configPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                v2rayProcess.OutputDataReceived += (outputSender, args) =>
                {
                    if (args.Data != null) LogMessage(args.Data);
                };
                v2rayProcess.ErrorDataReceived += (errorSender, args) =>
                {
                    if (args.Data != null) LogMessage("Error: " + args.Data);
                };

                v2rayProcess.Start();
                v2rayProcess.BeginOutputReadLine();
                v2rayProcess.BeginErrorReadLine();

                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
                LogMessage("V2Ray Connected!");

                SetSystemProxy("127.0.0.1", 10808);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start V2Ray: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (v2rayProcess != null && !v2rayProcess.HasExited)
            {
                v2rayProcess.Kill();
                v2rayProcess = null;

                ConnectButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;
                LogMessage("V2Ray Disconnected!");

                ResetSystemProxy();
            }
        }

        private void LogMessage(string message)
        {
            if (mainWindow != null)
            {
                mainWindow.LogMessage(message);
            }
        }

        private void SetSystemProxy(string proxyAddress, int proxyPort)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
                if (key != null)
                {
                    key.SetValue("ProxyEnable", 1);
                    key.SetValue("ProxyServer", $"socks={proxyAddress}:{proxyPort}");
                    LogMessage("System Proxy Set to V2Ray: " + proxyAddress + ":" + proxyPort);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set system proxy: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetSystemProxy()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
                if (key != null)
                {
                    key.SetValue("ProxyEnable", 0);
                    key.SetValue("ProxyServer", "");
                    LogMessage("System Proxy Reset.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to reset system proxy: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
