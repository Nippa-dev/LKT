using Renci.SshNet;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace LKtunnel
{
    public partial class SSH : UserControl, IConfigurableProtocol
    {
        private SshClient sshClient;
        private ForwardedPortDynamic portForwarding;
        private bool isConnected = false;
        public TextBox MainWindowLogsTextBox { get; set; }

        public SSH()
        {
            InitializeComponent();
        }

        // Apply config from .lktconf
        public void ApplyConfig(ProtocolConfig config)
        {
            SSHHost.Text = config.SSHHost;
            SSHPort.Text = config.SSHPort;
            SSHUsername.Text = config.SSHUsername;
            SSHPassword.Password = config.SSHPassword;
        }

        // Export config to .lktconf
        public ProtocolConfig ExportConfig()
        {
            return new ProtocolConfig
            {
                Protocol = "SSH Tunneling",
                SSHHost = SSHHost.Text,
                SSHPort = SSHPort.Text,
                SSHUsername = SSHUsername.Text,
                SSHPassword = SSHPassword.Password
            };
        }

        private void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now}: {message}");
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
                Log("Please fill all the fields.");
                return;
            }

            if (!int.TryParse(SSHPort.Text, out port))
            {
                Log("Invalid port number.");
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
                    Log("SSH Disconnected!");
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

        private class SshConfig
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private void SaveConfig()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                Title = "Save SSH Configuration"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    SshConfig config = new SshConfig
                    {
                        Host = SSHHost.Text,
                        Port = int.TryParse(SSHPort.Text, out int port) ? port : 22,
                        Username = SSHUsername.Text,
                        Password = SSHPassword.Password
                    };

                    string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                    File.WriteAllText(saveFileDialog.FileName, json);
                    Log($"Configuration saved successfully to {saveFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    Log($"Error saving configuration: {ex.Message}");
                }
            }
        }

        private void LoadConfig()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                Title = "Load SSH Configuration"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(openFileDialog.FileName);
                    SshConfig config = JsonConvert.DeserializeObject<SshConfig>(json);

                    SSHHost.Text = config.Host;
                    SSHPort.Text = config.Port.ToString();
                    SSHUsername.Text = config.Username;
                    SSHPassword.Password = config.Password;

                    Log($"Configuration loaded successfully from {openFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    Log($"Error loading configuration: {ex.Message}");
                }
            }
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
        }

        private void LoadConfig_Click(object sender, RoutedEventArgs e)
        {
            LoadConfig();
        }
    }
}
