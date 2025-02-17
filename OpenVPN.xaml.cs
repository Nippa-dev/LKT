using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace LKtunnel
{
    public partial class OpenVPN : UserControl
    {
        private Process openvpnProcess;

        public OpenVPN()
        {
            InitializeComponent();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            string openvpnPath = @"C:\Program Files\OpenVPN\bin\openvpn.exe";
            string configPath = @"C:\Users\klnip\Downloads\sshmax-nipun_at1-udp.ovpn";

            if (!File.Exists(openvpnPath) || !File.Exists(configPath))
            {
                MessageBox.Show("OpenVPN executable or config file missing!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                openvpnProcess.Start();
                MessageBox.Show("OpenVPN Connected!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
                openvpnProcess.Kill();
                openvpnProcess = null;
                MessageBox.Show("OpenVPN Disconnected!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
