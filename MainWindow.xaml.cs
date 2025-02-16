using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace LKtunnel
{
    public partial class MainWindow : Window
    {
        private Process openvpnProcess;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectVPN_Click(object sender, RoutedEventArgs e)
        {
            string openvpnPath = @"C:\Program Files\OpenVPN\bin\openvpn.exe";
            string configPath = @"Configs\vpn_config.ovpn";

            if (!File.Exists(openvpnPath))
            {
                MessageBox.Show("OpenVPN is not installed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(configPath))
            {
                MessageBox.Show("VPN config file not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                openvpnProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = openvpnPath,
                        Arguments = $"--config \"{configPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                openvpnProcess.OutputDataReceived += (s, ev) => Dispatcher.Invoke(() => LogOutput(ev.Data));
                openvpnProcess.ErrorDataReceived += (s, ev) => Dispatcher.Invoke(() => LogOutput(ev.Data));

                openvpnProcess.Start();
                openvpnProcess.BeginOutputReadLine();
                openvpnProcess.BeginErrorReadLine();

                MessageBox.Show("VPN Connected!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start VPN: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisconnectVPN_Click(object sender, RoutedEventArgs e)
        {
            if (openvpnProcess != null && !openvpnProcess.HasExited)
            {
                openvpnProcess.Kill();
                openvpnProcess = null;
                MessageBox.Show("VPN Disconnected!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LogOutput(string log)
        {
            if (!string.IsNullOrEmpty(log))
            {
                Console.WriteLine(log); // You can redirect this to a UI text box
            }
        }
    }
}
