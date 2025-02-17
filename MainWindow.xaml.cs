using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace LKtunnel
{
    public partial class MainWindow : Window
    {
        private bool isDarkMode; // Track whether dark mode or light mode is active
        private bool isUserThemeSelected = false; // Track if the user has selected a theme

        public MainWindow()
        {
            InitializeComponent();
            DetectSystemTheme();
            ApplyTheme();
        }

        // Detect the system theme (light or dark)
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

        // Apply the theme based on current mode
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

        // Close the application when the custom close button is clicked
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    



private void LoadPage(UserControl page)
        {
            MainContent.Content = page;
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            LoadPage(new Dashboard());
        }

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
