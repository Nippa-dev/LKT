using System;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LKtunnel
{
    public partial class Dashboard : UserControl
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private bool isRefreshing = false; // Prevents multiple refresh actions

        public Dashboard()
        {
            InitializeComponent();
            RefreshStatus();
        }

        private async void RefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            if (isRefreshing) return; // Avoid multiple refreshes

            isRefreshing = true;
            RefreshButton.IsEnabled = false;
            RefreshButton.Content = "Refreshing...";

            await RefreshStatus();

            RefreshButton.Content = "Refresh";
            RefreshButton.IsEnabled = true;
            isRefreshing = false;
        }

        private async Task RefreshStatus()
        {
            try
            {
                string publicIp = await GetPublicIpAsync();
                string localNetworkIp = GetLocalNetworkIp();
                string adapterName = GetActiveNetworkAdapter();

                bool vpnConnected = IsVpnConnected(publicIp, localNetworkIp);

                // Update UI safely
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IpAddressText.Text = $"Public IP: {publicIp}\nLocal IP: {localNetworkIp}\nAdapter: {adapterName}";
                    VpnStatusText.Text = vpnConnected ? "Connected" : "Disconnected";
                    VpnStatusText.Foreground = vpnConnected ? Brushes.Green : Brushes.Red;
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IpAddressText.Text = "Error Fetching IP";
                });
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task<string> GetPublicIpAsync()
        {
            string[] urls = { "https://api.ipify.org", "https://checkip.amazonaws.com", "https://ifconfig.me" };
            foreach (var url in urls)
            {
                try
                {
                    return (await httpClient.GetStringAsync(url)).Trim();
                }
                catch { /* Try the next one */ }
            }
            return "Unavailable";
        }

        private string GetLocalNetworkIp()
        {
            try
            {
                return NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                                n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                n.GetIPProperties().UnicastAddresses.Any(a => a.Address.AddressFamily == AddressFamily.InterNetwork))
                    .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                    .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork) // IPv4 only
                    .Select(a => a.Address.ToString())
                    .FirstOrDefault() ?? "Unavailable";
            }
            catch
            {
                return "Unavailable";
            }
        }

        private string GetActiveNetworkAdapter()
        {
            try
            {
                return NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                                n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                n.GetIPProperties().UnicastAddresses.Any(a => a.Address.AddressFamily == AddressFamily.InterNetwork))
                    .Select(n => n.Name)
                    .FirstOrDefault() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private bool IsVpnConnected(string publicIp, string localIp)
        {
            if (string.IsNullOrEmpty(publicIp) || string.IsNullOrEmpty(localIp) || publicIp == "Unavailable")
                return false;

            // Check if the public IP is in a private range
            bool isPublicIpPrivate = publicIp.StartsWith("192.168.") ||
                                     publicIp.StartsWith("10.") ||
                                     publicIp.StartsWith("172.16.");

            // If the public IP is private or matches local IP, assume not connected to a VPN
            if (isPublicIpPrivate || publicIp == localIp)
                return false;

            // Check if a VPN adapter is active
            return NetworkInterface.GetAllNetworkInterfaces()
              .Any(n => n.OperationalStatus == OperationalStatus.Up &&
              (n.Description.IndexOf("VPN", StringComparison.OrdinalIgnoreCase) >= 0 ||
               n.Name.IndexOf("VPN", StringComparison.OrdinalIgnoreCase) >= 0));
        }
    }
}