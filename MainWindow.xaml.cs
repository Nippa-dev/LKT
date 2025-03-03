using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace LKtunnel
{
    public partial class MainWindow : Window
    {
        private bool isConnected = false; // Track connection status
        private UserControl currentProtocolPage;
        private bool isDarkMode;
        private bool isUserThemeSelected = false;

        private SSH sshControl; // Declare SSH control for access
        private bool isVpnConnected = false; // Track VPN connection status

        public MainWindow()
        {
            InitializeComponent();
            DetectSystemTheme();
            ApplyTheme();
            /// Initialize SSH control on startup
            sshControl = new SSH();

            // Set the reference of MainWindow's LogsTextBox to the SSH control
            sshControl.MainWindowLogsTextBox = LogsTextBox;
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
            if (isVpnConnected)
            {
                // Disconnect VPN (call SSH disconnect)
                sshControl.Disconnect_Click(sender, e);
                ConnectDisconnectButton.Content = "Connect VPN";
                LogMessage("Disconnected from SSH and VPN.");
            }
            else
            {
                // Connect VPN (call SSH connect)
                sshControl.Connect_Click(sender, e);
                ConnectDisconnectButton.Content = "Disconnect VPN";
                LogMessage("Connected to SSH and VPN started.");
            }

            isVpnConnected = !isVpnConnected; // Toggle VPN connection status
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
        private void LoadProtocolPage(string protocol)
        {
            // Remove existing content
            MainContent.Content = null;

            // Load the correct page based on the selected protocol
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
                    MainContent.Content = new V2Ray(); // Load V2Ray UserControl
                    break;

                case "SSH Tunneling":
                    MainContent.Content = sshControl; // Use SSH control for SSH tunneling
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

        // Load a page based on the button click
        private void LoadPage(UserControl page)
        {
            MainContent.Content = page;
        }


        // Navigation for Dashboard
        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            // Add your logic for the Dashboard button click event here
            LoadPage(new Dashboard());
        }
        // Dark Mode Toggle Button Checked (When Dark Mode is activated)
        // Dark Mode Toggle Button Checked (When Dark Mode is activated)
        private void DarkModeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            // Switch to Dark Mode
            isDarkMode = true;
            ApplyTheme();  // Apply the new theme

            // Change the button text to "Light Mode"
            DarkModeToggleButton.Content = "Light Mode";
        }

        // Dark Mode Toggle Button Unchecked (When Light Mode is activated)
        private void DarkModeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            // Switch to Light Mode
            isDarkMode = false;
            ApplyTheme();  // Apply the new theme

            // Change the button text to "Dark Mode"
            DarkModeToggleButton.Content = "Dark Mode";
        }

        private void LogsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
