using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace LKtunnel
{
    public partial class V2Ray : UserControl
    {
        private Process v2rayProcess;
        private MainWindow mainWindow; // Reference to MainWindow

        public V2Ray()
        {
            InitializeComponent();
            mainWindow = Application.Current.MainWindow as MainWindow; // Get reference to MainWindow
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

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            string v2rayPath = @"C:\Users\klnip\source\repos\LKT\bin\V2Ray\v2ray.exe"; // Update to your correct path
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
                        Arguments = $"run -config \"{configPath}\"", // Correct command
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                // Capture V2Ray output
                v2rayProcess.OutputDataReceived += (outputSender, args) =>
                {
                    if (args.Data != null)
                    {
                        LogMessage(args.Data); // Log output with timestamp
                    }
                };
                v2rayProcess.ErrorDataReceived += (errorSender, args) =>
                {
                    if (args.Data != null)
                    {
                        LogMessage("Error: " + args.Data); // Log errors with timestamp
                    }
                };

                v2rayProcess.Start();
                v2rayProcess.BeginOutputReadLine();
                v2rayProcess.BeginErrorReadLine();

                ConnectButton.IsEnabled = false; // Disable Connect button
                DisconnectButton.IsEnabled = true; // Enable Disconnect button
                LogMessage("V2Ray Connected!");

                // Set system proxy to route traffic through V2Ray's local SOCKS proxy (127.0.0.1:10808)
                SetSystemProxy("127.0.0.1", 10808); // Set the proxy port to 10808
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start V2Ray: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (v2rayProcess != null && !v2rayProcess.HasExited)
            {
                v2rayProcess.Kill();
                v2rayProcess = null;

                ConnectButton.IsEnabled = true; // Enable Connect button
                DisconnectButton.IsEnabled = false; // Disable Disconnect button

                LogMessage("V2Ray Disconnected!");

                // Reset system proxy settings when disconnecting
                ResetSystemProxy();
            }
        }

        private void LogMessage(string message)
        {
            // Check if the reference to MainWindow is valid and log the message to LogsTextBox
            if (mainWindow != null)
            {
                mainWindow.LogMessage(message); // Call LogMessage method in MainWindow
            }
        }

        private void SetSystemProxy(string proxyAddress, int proxyPort)
        {
            try
            {
                // Open registry key for proxy settings
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true);

                if (key != null)
                {
                    key.SetValue("ProxyEnable", 1); // Enable proxy
                    key.SetValue("ProxyServer", $"socks={proxyAddress}:{proxyPort}"); // Set proxy address and port

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
                // Open registry key for proxy settings
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true);

                if (key != null)
                {
                    key.SetValue("ProxyEnable", 0); // Disable proxy
                    key.SetValue("ProxyServer", ""); // Clear proxy server setting

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
