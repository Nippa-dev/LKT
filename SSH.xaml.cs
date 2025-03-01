using Renci.SshNet;
using System;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;  // For registry access

namespace LKtunnel
{
    public partial class SSH : UserControl
    {
        private SshClient sshClient;
        private ForwardedPortDynamic portForwarding;
        private bool isConnected = false;

        public SSH()
        {
            InitializeComponent();
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            SSHHost.Text = "sg10.vpnjantit.com"; // Default server host
            SSHPort.Text = "22"; // Default SSH port
            SSHUsername.Text = "nipun-vpnjantit.com"; // Default SSH username
            SSHPassword.Password = "nipun"; // Default SSH password
        }

        // Connect SSH and set up SOCKS proxy
        public void Connect_Click(object sender, RoutedEventArgs e)
        {
            // Check if VPN is connected before proceeding
            if (!IsVpnConnected())
            {
                MessageBox.Show("VPN is not connected! Please connect your VPN first.", "VPN Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string host = SSHHost.Text;
            int port;
            string username = SSHUsername.Text;
            string password = SSHPassword.Password;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill all the fields", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(SSHPort.Text, out port))
            {
                MessageBox.Show("Invalid port number", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Disconnect first if already connected
            DisconnectSSH();

            try
            {
                sshClient = new SshClient(host, port, username, password);
                sshClient.Connect();
                isConnected = true;
                MessageBox.Show("SSH Connected!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Set up dynamic port forwarding (SOCKS proxy) on port 1027
                portForwarding = new ForwardedPortDynamic("127.0.0.1", 1027);
                sshClient.AddForwardedPort(portForwarding);
                portForwarding.Start();

                MessageBox.Show("SOCKS Proxy started on 127.0.0.1:1027", "Proxy Info", MessageBoxButton.OK, MessageBoxImage.Information);

                // Set system proxy to 127.0.0.1:1027 automatically
                SetSystemProxy("127.0.0.1", 1027); // Set the proxy

            }
            catch (Exception ex)
            {
                MessageBox.Show($"SSH Connection Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                DisconnectSSH(); // Cleanup in case of failure
            }
        }

        // Disconnect SSH and stop proxy
        public void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            DisconnectSSH();
        }

        private void DisconnectSSH()
        {
            if (sshClient != null && sshClient.IsConnected)
            {
                try
                {
                    portForwarding?.Stop();
                    portForwarding?.Dispose();
                    sshClient.Disconnect();
                    sshClient.Dispose();
                    isConnected = false;

                    // Reset the proxy settings (optional)
                    ResetSystemProxy();

                    MessageBox.Show("SSH Disconnected and Proxy Stopped!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while disconnecting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("SSH is not connected!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Function to check if VPN is connected
        private bool IsVpnConnected()
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    var reply = ping.Send("8.8.8.8", 1000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        // Function to set the system proxy to 127.0.0.1:1027 for SOCKS
        private void SetSystemProxy(string host, int port)
        {
            try
            {
                // Modify the registry to set system proxy for SOCKS
                string proxySettingKey = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(proxySettingKey, true))
                {
                    if (key != null)
                    {
                        key.SetValue("ProxyEnable", 1);
                        key.SetValue("ProxyServer", $"socks={host}:{port}");
                    }
                }
                MessageBox.Show($"System Proxy Set to {host}:{port}", "Proxy Set", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting system proxy: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Function to reset the system proxy (optional)
        private void ResetSystemProxy()
        {
            try
            {
                string proxySettingKey = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(proxySettingKey, true))
                {
                    if (key != null)
                    {
                        key.SetValue("ProxyEnable", 0); // Disable the system proxy
                    }
                }
                MessageBox.Show("System Proxy Reset", "Proxy Reset", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting system proxy: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
