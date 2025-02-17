using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace LKtunnel
{
    public partial class Shadowsocks : UserControl
    {
        private Process shadowsocksProcess;

        public Shadowsocks()
        {
            InitializeComponent();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            string ssPath = @"C:\Program Files\Shadowsocks\shadowsocks.exe";

            if (!File.Exists(ssPath))
            {
                MessageBox.Show("Shadowsocks is not installed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string server = ShadowsocksServer.Text;
            string port = ShadowsocksPort.Text;
            string password = ShadowsocksPassword.Password;

            try
            {
                shadowsocksProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ssPath,
                        Arguments = $"-s {server} -p {port} -k {password} -m aes-256-gcm",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                shadowsocksProcess.Start();
                MessageBox.Show("Shadowsocks Connected!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start Shadowsocks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (shadowsocksProcess != null && !shadowsocksProcess.HasExited)
            {
                shadowsocksProcess.Kill();
                shadowsocksProcess = null;
                MessageBox.Show("Shadowsocks Disconnected!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
