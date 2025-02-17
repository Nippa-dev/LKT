using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace LKtunnel
{
    public partial class Dashboard : UserControl
    {
        public Dashboard()
        {
            InitializeComponent();
            RefreshStatus();
        }

        private void RefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            try
            {
                // Simulating VPN status check
                bool vpnConnected = CheckVPNStatus();
                VpnStatusText.Text = vpnConnected ? "Connected" : "Disconnected";
                VpnStatusText.Foreground = vpnConnected ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;

                // Fetch public IP address
                string publicIp = new WebClient().DownloadString("https://api.ipify.org").Trim();
                IpAddressText.Text = publicIp;
            }
            catch (Exception)
            {
                IpAddressText.Text = "Error Fetching IP";
            }
        }

        private bool CheckVPNStatus()
        {
            // Here you can add a real VPN status check using OpenVPN, WireGuard, etc.
            return false; // Default to "Disconnected"
        }
    }
}
