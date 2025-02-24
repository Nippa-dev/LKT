using Renci.SshNet;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LKtunnel
{
    public partial class SSH : UserControl
    {
        private SshClient sshClient;
        private ForwardedPortDynamic portForwarding;

        public SSH()
        {
            InitializeComponent();
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            SSHHost.Text = "sg1.vpnjantit.com"; // Default server host
            SSHPort.Text = "22"; // Default SSH port
            SSHUsername.Text = "nipun-vpnjantit.com"; // Default SSH username
            SSHPassword.Password = "nipun"; // Default SSH password
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
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

            // Initialize SSH client and attempt connection
            sshClient = new SshClient(host, port, username, password);

            try
            {
                sshClient.Connect();
                MessageBox.Show("SSH Connected!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Set up dynamic port forwarding (SOCKS proxy)
                portForwarding = new ForwardedPortDynamic("127.0.0.1", 1080); // Local SOCKS proxy on port 1080
                sshClient.AddForwardedPort(portForwarding);
                portForwarding.Start();
                MessageBox.Show("SOCKS Proxy started on 127.0.0.1:1080", "Proxy Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SSH Connection Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (sshClient != null && sshClient.IsConnected)
            {
                // Stop port forwarding and disconnect
                portForwarding?.Stop();
                sshClient.Disconnect();
                MessageBox.Show("SSH Disconnected and Proxy Stopped!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("SSH is not connected!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
