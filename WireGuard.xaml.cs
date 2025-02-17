using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace LKtunnel
{
    public partial class WireGuard : UserControl
    {
        private Process wireguardProcess;

        public WireGuard()
        {
            InitializeComponent();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            string wireguardPath = @"C:\Program Files\WireGuard\wg.exe";
            string configPath = @"C:\Users\klnip\Downloads\wireguard.conf";

            if (!File.Exists(wireguardPath) || !File.Exists(configPath))
            {
                MessageBox.Show("WireGuard executable or config file missing!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                wireguardProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = wireguardPath,
                        Arguments = $"quick up \"{configPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                wireguardProcess.Start();
                MessageBox.Show("WireGuard Connected!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start WireGuard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (wireguardProcess != null && !wireguardProcess.HasExited)
            {
                wireguardProcess.Kill();
                wireguardProcess = null;
                MessageBox.Show("WireGuard Disconnected!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
