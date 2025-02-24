using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace LKtunnel
{
    public partial class MainWindow : Window
    {
        private bool isDarkMode;
        private bool isUserThemeSelected = false;

        public MainWindow()
        {
            InitializeComponent();
            DetectSystemTheme();
            ApplyTheme();
            LoadLogsPage(); // Set Logs as default page on startup
        }

        // Detect system theme and apply it to the app
        private void DetectSystemTheme()
        {
            var registryKey = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", null);
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

        // Toggle the theme when the button is clicked
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

        // Load the Logs page as the default page
        private void LoadLogsPage()
        {
            MainContent.Content = new Logs();
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
                    MainContent.Content = new OpenVPN();
                    break;
                case "WireGuard":
                    MainContent.Content = new WireGuard();
                    break;
                case "Shadowsocks":
                    MainContent.Content = new Shadowsocks();
                    break;
                case "V2Ray":
                    MainContent.Content = new V2Ray();
                    break;
                case "SSH Tunneling":
                    MainContent.Content = new SSH();
                    break;
                default:
                    break;
            }
        }

        // Load a page based on the button click
        private void LoadPage(UserControl page)
        {
            MainContent.Content = page;
        }

        // Navigation for Dashboard
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            LoadPage(new Dashboard());
        }

        // Optional: You could remove these if you're using ComboBox exclusively to switch pages.
        private void OpenVPN_Click(object sender, RoutedEventArgs e)
        {
            LoadPage(new OpenVPN());
        }

        private void WireGuard_Click(object sender, RoutedEventArgs e)
        {
            LoadPage(new WireGuard());
        }

        private void Shadowsocks_Click(object sender, RoutedEventArgs e)
        {
            LoadPage(new Shadowsocks());
        }

        private void V2Ray_Click(object sender, RoutedEventArgs e)
        {
            LoadPage(new V2Ray());
        }

        private void SSH_Click(object sender, RoutedEventArgs e)
        {
            LoadPage(new SSH());
        }

        private void Logs_Click(object sender, RoutedEventArgs e)
        {
            LoadPage(new Logs());
        }

    }
}