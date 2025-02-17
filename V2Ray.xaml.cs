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

        public V2Ray()
        {
            InitializeComponent();
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
            string v2rayPath = @"C:\Program Files\V2Ray\v2ray.exe";
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
                        Arguments = $"-config \"{configPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                v2rayProcess.Start();
                MessageBox.Show("V2Ray Connected!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show("V2Ray Disconnected!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
