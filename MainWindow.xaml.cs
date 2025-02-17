using System;
using System.Windows;
using System.Windows.Controls;
using Org.BouncyCastle.Utilities;

namespace LKtunnel
{
    public partial class MainWindow : Window
    {
        private bool isDarkMode = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ToggleDarkMode_Click(object sender, RoutedEventArgs e)
        {
            if (isDarkMode)
            {
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative) });
                isDarkMode = false;
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative) });
                isDarkMode = true;
            }
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
