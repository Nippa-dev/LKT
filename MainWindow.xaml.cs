using System;
using System.Windows;
using System.Windows.Controls;

namespace LKtunnel
{
    public partial class MainWindow : Window
    {
        private bool isConnected = false; // Track connection status
        private UserControl currentProtocolPage;
        private bool isDarkMode;
        private bool isUserThemeSelected = false;

        private SSH sshControl; // Declare SSH control for access
        private V2Ray v2rayControl; // Declare V2Ray control for access
        private bool isVpnConnected = false; // Track VPN connection status

        public MainWindow()
        {
            InitializeComponent();
            DetectSystemTheme();
            ApplyTheme();
            // Initialize SSH and V2Ray controls
            sshControl = new SSH();
            v2rayControl = new V2Ray();

            // Set the reference of MainWindow's LogsTextBox to the SSH and V2Ray controls
            sshControl.MainWindowLogsTextBox = LogsTextBox;
            v2rayControl.MainWindowLogsTextBox = LogsTextBox;
        }

        // Detect system theme and apply it to the app
        private void DetectSystemTheme()
        {
            var registryKey = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", null);
            if (registryKey != null)
            {
                isDarkMode = (int)registryKey == 0; // 0 = Dark mode, 1 = Light mode
            }
            else
            {
                isDarkMode = true; // Default to dark mode if registry key is missing
            }
        }

        // Apply the current theme (dark or light mode)
        private void ApplyTheme()
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            string theme = isDarkMode ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";
            var resourceDictionary = new ResourceDictionary() { Source = new Uri(theme, UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        // Toggle between dark mode and light mode when the button is clicked
        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            if (!isUserThemeSelected)
            {
                // Disable system theme detection and allow user to toggle themes
                isUserThemeSelected = true;
            }

            // Toggle dark mode or light mode
            isDarkMode = !isDarkMode;
            ApplyTheme();
        }

        // Connect/Disconnect button logic
        private void ConnectDisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected protocol from the ComboBox
            ComboBoxItem selectedItem = ProtocolComboBox.SelectedItem as ComboBoxItem;
            string selectedProtocol = selectedItem?.Content.ToString();

            if (string.IsNullOrEmpty(selectedProtocol))
            {
                LogMessage("No protocol selected. Please select a protocol.");
                return;
            }

            if (isVpnConnected)
            {
                // Disconnect based on selected protocol
                switch (selectedProtocol)
                {
                    case "OpenVPN":
                        DisconnectOpenVPN();
                        break;

                    case "WireGuard":
                        DisconnectWireGuard();
                        break;

                    case "Shadowsocks":
                        DisconnectShadowsocks();
                        break;

                    case "V2Ray":
                        v2rayControl.Disconnect_Click(sender, e);  // Call Disconnect for V2Ray
                        break;

                    case "SSH Tunneling":
                        sshControl.Disconnect_Click(sender, e);  // Call Disconnect for SSH
                        break;

                    default:
                        LogMessage("Unknown protocol selected.");
                        return;
                }

                ConnectDisconnectButton.Content = "Connect VPN";
                LogMessage("Disconnected from the selected protocol.");
            }
            else
            {
                // Connect based on selected protocol
                switch (selectedProtocol)
                {
                    case "OpenVPN":
                        ConnectOpenVPN();
                        break;

                    case "WireGuard":
                        ConnectWireGuard();
                        break;

                    case "Shadowsocks":
                        ConnectShadowsocks();
                        break;

                    case "V2Ray":
                        v2rayControl.Connect_Click(sender, e);  // Call Connect for V2Ray
                        break;

                    case "SSH Tunneling":
                        sshControl.Connect_Click(sender, e);  // Call Connect for SSH
                        break;

                    default:
                        LogMessage("Unknown protocol selected.");
                        return;
                }

                ConnectDisconnectButton.Content = "Disconnect VPN";
                LogMessage("Connected to the selected protocol.");
            }

            // Toggle VPN connection status
            isVpnConnected = !isVpnConnected;
        }


        // Handle protocol selection from ComboBox
        private void ProtocolComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = ProtocolComboBox.SelectedItem as ComboBoxItem;

            if (selectedItem != null)
            {
                string selectedProtocol = selectedItem.Content.ToString();
                LoadProtocolPage(selectedProtocol);
            }
        }

        // Load the page corresponding to the selected protocol
        // Load the page corresponding to the selected protocol
        private void LoadProtocolPage(string protocol)
        {
            MainContent.Content = null; // Clear the current content

            switch (protocol)
            {
                case "OpenVPN":
                    MainContent.Content = new OpenVPN(); // Load OpenVPN UserControl
                    break;

                case "WireGuard":
                    MainContent.Content = new WireGuard(); // Load WireGuard UserControl
                    break;

                case "Shadowsocks":
                    MainContent.Content = new Shadowsocks(); // Load Shadowsocks UserControl
                    break;

                case "V2Ray":
                    MainContent.Content = v2rayControl; // V2Ray control
                    break;

                case "SSH Tunneling":
                    MainContent.Content = sshControl; // SSH control
                    break;

                default:
                    break;
            }
        }


        // Log message to the TextBox
        public void LogMessage(string message)
        {
            if (LogsTextBox.Dispatcher.CheckAccess())
            {
                // Append the log message with a timestamp
                LogsTextBox.AppendText($"[{DateTime.Now}] {message}{Environment.NewLine}");
                LogsTextBox.ScrollToEnd(); // Ensure the latest log is visible
            }
            else
            {
                LogsTextBox.Dispatcher.Invoke(() => LogMessage(message));
            }
        }

        // Example Methods for each protocol connection/disconnection

        private void ConnectOpenVPN()
        {
            LogMessage("Connecting to OpenVPN...");
            // Implement actual OpenVPN connection logic here
        }

        private void DisconnectOpenVPN()
        {
            LogMessage("Disconnecting from OpenVPN...");
            // Implement actual OpenVPN disconnection logic here
        }

        private void ConnectWireGuard()
        {
            LogMessage("Connecting to WireGuard...");
            // Implement actual WireGuard connection logic here
        }

        private void DisconnectWireGuard()
        {
            LogMessage("Disconnecting from WireGuard...");
            // Implement actual WireGuard disconnection logic here
        }

        private void ConnectShadowsocks()
        {
            LogMessage("Connecting to Shadowsocks...");
            // Implement actual Shadowsocks connection logic here
        }

        private void DisconnectShadowsocks()
        {
            LogMessage("Disconnecting from Shadowsocks...");
            // Implement actual Shadowsocks disconnection logic here
        }

        private void ConnectV2Ray()
        {
            LogMessage("Connecting to V2Ray...");
            // Implement actual V2Ray connection logic here
        }

        private void DisconnectV2Ray()
        {
            LogMessage("Disconnecting from V2Ray...");
            // Implement actual V2Ray disconnection logic here
        }

        // Load a page based on the button click
        private void LoadPage(UserControl page)
        {
            MainContent.Content = page;
        }

        // Navigation for Dashboard
        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPage(new Dashboard());
        }

        // Dark Mode Toggle Button Checked (When Dark Mode is activated)
        private void DarkModeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            isDarkMode = true;
            ApplyTheme();  // Apply the new theme
            DarkModeToggleButton.Content = "Light Mode";
        }

        // Dark Mode Toggle Button Unchecked (When Light Mode is activated)
        private void DarkModeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            isDarkMode = false;
            ApplyTheme();  // Apply the new theme
            DarkModeToggleButton.Content = "Dark Mode";
        }

        private void LogsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}
