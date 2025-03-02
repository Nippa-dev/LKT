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

        // Add public property for MainWindow's LogsTextBox
        public TextBox MainWindowLogsTextBox { get; set; }

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

        // Function to log messages to MainWindow's LogsTextBox
        private void Log(string message)
        {
            // Log in the MainWindow's LogsTextBox if it's provided
            if (MainWindowLogsTextBox != null)
            {
                MainWindowLogsTextBox.Text += $"{DateTime.Now}: {message}\n";
                MainWindowLogsTextBox.ScrollToEnd(); // Automatically scroll to the bottom
            }
        }

        // Connect SSH and set up SOCKS proxy
        public void Connect_Click(object sender, RoutedEventArgs e)
        {
            // Check if VPN is connected before proceeding
            if (!IsVpnConnected())
            {
                Log("VPN is not connected! Please connect your VPN first.");
                return;
            }

            string host = SSHHost.Text;
            int port;
            string username = SSHUsername.Text;
            string password = SSHPassword.Password;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Log("Please fill all the fields");
                return;
            }

            if (!int.TryParse(SSHPort.Text, out port))
            {
                Log("Invalid port number");
                return;
            }

            // Disconnect first if already connected
            DisconnectSSH();

            try
            {
                sshClient = new SshClient(host, port, username, password);
                sshClient.Connect();
                isConnected = true;
                Log("SSH Connected!");

                // Set up dynamic port forwarding (SOCKS proxy) on port 1027
                portForwarding = new ForwardedPortDynamic("127.0.0.1", 1027);
                sshClient.AddForwardedPort(portForwarding);
                portForwarding.Start();

                Log("SOCKS Proxy started on 127.0.0.1:1027");

                // Set system proxy to 127.0.0.1:1027 automatically
                SetSystemProxy("127.0.0.1", 1027); // Set the proxy
            }
            catch (Exception ex)
            {
                Log($"SSH Connection Failed: {ex.Message}");
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

                    Log("SSH Disconnected and Proxy Stopped!");
                }
                catch (Exception ex)
                {
                    Log($"Error while disconnecting: {ex.Message}");
                }
            }
            else
            {
                Log("SSH is not connected!");
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
                Log($"System Proxy Set to {host}:{port}");
            }
            catch (Exception ex)
            {
                Log($"Error setting system proxy: {ex.Message}");
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
                Log("System Proxy Reset");
            }
            catch (Exception ex)
            {
                Log($"Error resetting system proxy: {ex.Message}");
            }
        }
    }
}
