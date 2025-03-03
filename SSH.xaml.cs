using Renci.SshNet;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;

namespace LKtunnel
{
    public partial class SSH : UserControl
    {
        private SshClient sshClient;
        private ForwardedPortDynamic portForwarding;
        private bool isConnected = false;
        private DispatcherTimer vpnCheckTimer; // Timer to check VPN status

        public TextBox MainWindowLogsTextBox { get; set; }

        public SSH()
        {
            InitializeComponent();
            SetDefaultValues();
            StartVpnMonitoring();

            // Handle application closing event
            Application.Current.Exit += OnApplicationExit;
        }

        private void SetDefaultValues()
        {
            SSHHost.Text = "sg10.vpnjantit.com";
            SSHPort.Text = "22";
            SSHUsername.Text = "nipun-vpnjantit.com";
            SSHPassword.Password = "nipun";
        }

        private void Log(string message)
        {
            if (MainWindowLogsTextBox != null)
            {
                MainWindowLogsTextBox.Text += $"{DateTime.Now}: {message}\n";
                MainWindowLogsTextBox.ScrollToEnd();
            }
        }

        public void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (!IsVpnConnected())
            {
                Log("VPN is not connected! Please connect your VPN first.");
                return;
            }

            string host = SSHHost.Text;
            int port;
            string username = SSHUsername.Text;
            string password = SSHPassword.Password;

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

            DisconnectSSH();

            try
            {
                sshClient = new SshClient(host, port, username, password);
                sshClient.Connect();
                isConnected = true;
                Log("SSH Connected!");

                portForwarding = new ForwardedPortDynamic("127.0.0.1", 1027);
                sshClient.AddForwardedPort(portForwarding);
                portForwarding.Start();

                Log("SOCKS Proxy started on 127.0.0.1:1027");

                SetSystemProxy("127.0.0.1", 1027);
            }
            catch (Exception ex)
            {
                Log($"SSH Connection Failed: {ex.Message}");
                DisconnectSSH();
            }
        }

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

        private bool IsVpnConnected()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "interface show interface",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    string[] vpnKeywords = { "vpn", "wireguard", "openvpn", "pptp", "l2tp", "sstp" };
                    return output.ToLower().Split('\n').Any(line => vpnKeywords.Any(vpn => line.Contains(vpn)));
                }
            }
            catch (Exception ex)
            {
                Log($"Error checking VPN status: {ex.Message}");
                return false;
            }
        }

        private void SetSystemProxy(string host, int port)
        {
            try
            {
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

        private void ResetSystemProxy()
        {
            try
            {
                string proxySettingKey = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(proxySettingKey, true))
                {
                    if (key != null)
                    {
                        key.SetValue("ProxyEnable", 0);
                    }
                }
                Log("System Proxy Reset");
            }
            catch (Exception ex)
            {
                Log($"Error resetting system proxy: {ex.Message}");
            }
        }

        private void StartVpnMonitoring()
        {
            vpnCheckTimer = new DispatcherTimer();
            vpnCheckTimer.Interval = TimeSpan.FromSeconds(5);
            vpnCheckTimer.Tick += VpnCheckTimer_Tick;
            vpnCheckTimer.Start();
        }

        private void VpnCheckTimer_Tick(object sender, EventArgs e)
        {
            if (!IsVpnConnected())
            {
                ResetSystemProxy();
                Log("VPN Disconnected! Proxy Reset.");
            }
        }

        // ✅ NEW: Handle Application Closing Event
        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            Log("Application is closing. Resetting proxy...");
            ResetSystemProxy(); // Reset proxy when app is closed
        }
    }
}
