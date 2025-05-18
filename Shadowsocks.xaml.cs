using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Microsoft.Win32; // Needed for Registry access

namespace LKtunnel
{
    public partial class ShadowSocks : UserControl, IConfigurableProtocol
    {
        private Process shadowSocksProcess;
        private MainWindow mainWindow;
        public TextBox MainWindowLogsTextBox { get; set; }

        public ShadowSocks()
        {
            InitializeComponent();
            mainWindow = Application.Current.MainWindow as MainWindow;
        }

        // Apply imported configuration
        public void ApplyConfig(ProtocolConfig config)
        {
            ShadowSocksConfigPath.Text = config.ShadowSocksConfigPath;
        }

        // Export current config
        public ProtocolConfig ExportConfig()
        {
            return new ProtocolConfig
            {
                Protocol = "Shadowsocks",
                ShadowSocksConfigPath = ShadowSocksConfigPath.Text
            };
        }

        private void BrowseConfig_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                Title = "Select ShadowSocks Configuration File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ShadowSocksConfigPath.Text = openFileDialog.FileName;
            }
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                Title = "Save ShadowSocks Config Path"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, ShadowSocksConfigPath.Text);
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
                Title = "Load ShadowSocks Config Path"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ShadowSocksConfigPath.Text = File.ReadAllText(openFileDialog.FileName);
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
            string shadowSocksPath = Path.Combine(appDirectory, @"ShadowSocks\ShadowSocks.exe");
            string configPath = ShadowSocksConfigPath.Text;

            if (!File.Exists(shadowSocksPath) || !File.Exists(configPath))
            {
                MessageBox.Show("ShadowSocks executable or config file missing!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                shadowSocksProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = shadowSocksPath,
                        Arguments = $"-c \"{configPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                shadowSocksProcess.OutputDataReceived += (outputSender, args) =>
                {
                    if (args.Data != null) LogMessage(args.Data);
                };
                shadowSocksProcess.ErrorDataReceived += (errorSender, args) =>
                {
                    if (args.Data != null) LogMessage("Error: " + args.Data);
                };

                shadowSocksProcess.Start();
                shadowSocksProcess.BeginOutputReadLine();
                shadowSocksProcess.BeginErrorReadLine();

                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
                LogMessage("ShadowSocks Connected!");

                SetSystemProxy("127.0.0.1", 1080); // Default port
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start ShadowSocks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (shadowSocksProcess != null && !shadowSocksProcess.HasExited)
            {
                shadowSocksProcess.Kill();
                shadowSocksProcess = null;

                ConnectButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;
                LogMessage("ShadowSocks Disconnected!");

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
                    LogMessage("System Proxy Set to ShadowSocks: " + proxyAddress + ":" + proxyPort);
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
