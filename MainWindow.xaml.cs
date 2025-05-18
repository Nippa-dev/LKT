using System;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using Newtonsoft.Json;  // Add this for JSON serialization/deserialization
using Microsoft.Win32;  // For OpenFileDialog and SaveFileDialog


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


        /// Handle protocol selection
        private void ProtocolComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedProtocol = (ProtocolComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(selectedProtocol)) return;

            // Load the corresponding protocol user control
            switch (selectedProtocol)
            {
                case "OpenVPN":
                    MainContent.Content = new OpenVPN();  // Load OpenVPN control
                    break;
                case "WireGuard":
                    MainContent.Content = new WireGuard();  // Load WireGuard control
                    break;
                case "Shadowsocks":
                    MainContent.Content = new ShadowSocks(); // Load Shadowsocks UserControl
                    break;
                case "SSH Tunneling":
                    MainContent.Content = new SSH();  // Load SSH control
                    break;
                case "V2Ray":
                    MainContent.Content = new V2Ray();  // Load V2Ray control
                    break;
                default:
                    MainContent.Content = null;
                    break;
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
                    MainContent.Content = new ShadowSocks(); // Load Shadowsocks UserControl
                    break;

                case "V2Ray":
                    MainContent.Content = new V2Ray(); // V2Ray control
                    break;

                case "SSH Tunneling":
                    MainContent.Content = new SSH(); // SSH control
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

        // Import configuration from a JSON file
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "LK Tunnel Config (*.lktconf)|*.lktconf|JSON Files (*.json)|*.json",
                Title = "Import VPN Configuration"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileContent = File.ReadAllText(openFileDialog.FileName);
                    ProtocolConfig config = null;

                    // 🔍 Try parse as unlocked JSON
                    try
                    {
                        config = JsonConvert.DeserializeObject<ProtocolConfig>(fileContent);
                    }
                    catch
                    {
                        // 🔐 Ask for decryption password if normal JSON fails
                        string password = PromptPassword("Enter password to unlock this configuration:");
                        try
                        {
                            string decrypted = ConfigEncryption.Decrypt(fileContent, password);
                            config = JsonConvert.DeserializeObject<ProtocolConfig>(decrypted);
                        }
                        catch (Exception ex)
                        {
                            LogMessage("Failed to decrypt or parse configuration: " + ex.Message);
                            return;
                        }
                    }

                    if (config == null || string.IsNullOrEmpty(config.Protocol))
                    {
                        LogMessage("Invalid configuration file.");
                        return;
                    }

                    // Set Protocol dropdown
                    foreach (ComboBoxItem item in ProtocolComboBox.Items)
                    {
                        if (item.Content.ToString() == config.Protocol)
                        {
                            ProtocolComboBox.SelectedItem = item;
                            break;
                        }
                    }

                    // Load into UserControl
                    ApplyConfig(config);
                    LogMessage("Configuration imported successfully.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Error importing configuration: {ex.Message}");
                }
            }
        }




        // Apply the imported configuration to the UI
        private void ApplyConfig(ProtocolConfig config)
        {
            // Set values to UI components based on the imported config
            ProtocolComboBox.SelectedItem = config.Protocol;
            // Apply specific protocol configuration logic
            switch (config.Protocol)
            {
                case "OpenVPN":
                    MainContent.Content = new OpenVPN();
                    (MainContent.Content as OpenVPN)?.ApplyConfig(config);  // Apply config to OpenVPN control
                    break;
                case "WireGuard":
                    MainContent.Content = new WireGuard();
                    (MainContent.Content as WireGuard)?.ApplyConfig(config);  // Apply config to WireGuard control
                    break;
                case "SSH Tunneling":
                    MainContent.Content = new SSH();
                    (MainContent.Content as SSH)?.ApplyConfig(config);  // Apply config to SSH control
                    break;
                case "V2Ray":
                    MainContent.Content = new V2Ray();
                    (MainContent.Content as V2Ray)?.ApplyConfig(config);  // Apply config to V2Ray control
                    break;
                default:
                    MainContent.Content = null; // Default to empty content if no valid protocol
                    break;
            }
        }



        // Export configuration to a JSON file
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to export config as LOCKED (encrypted)?",
                                         "Export Mode",
                                         MessageBoxButton.YesNoCancel,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel) return;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "LK Tunnel Config (*.lktconf)|*.lktconf",
                Title = "Save VPN Configuration"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    if (MainContent.Content is IConfigurableProtocol protocolControl)
                    {
                        ProtocolConfig config = protocolControl.ExportConfig();
                        config.Protocol = (ProtocolComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                        // ✅ Mark the config as locked or unlocked
                        config.IsLocked = (result == MessageBoxResult.Yes);

                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(config, Formatting.Indented);

                        if (config.IsLocked)
                        {
                            string key = PromptPassword("Enter a password to lock the config:");
                            string encrypted = ConfigEncryption.Encrypt(json, key);
                            File.WriteAllText(saveFileDialog.FileName, encrypted);
                        }
                        else
                        {
                            File.WriteAllText(saveFileDialog.FileName, json);
                        }

                        LogMessage("Configuration exported successfully.");
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("Export failed: " + ex.Message);
                }
            }
        }

        private string PromptPassword(string title)
        {
            var inputDialog = new PasswordDialog(title);
            if (inputDialog.ShowDialog() == true)
            {
                return inputDialog.PasswordInput;
            }
            return "";
        }
        private async void ImportFromCloud_Click(object sender, RoutedEventArgs e)
        {
            string url = PromptInput("Enter the full URL to the cloud config:");

            if (string.IsNullOrWhiteSpace(url))
            {
                LogMessage("Cloud config import cancelled.");
                return;
            }

            try
            {
                using (var client = new System.Net.WebClient())
                {
                    string fileContent = await client.DownloadStringTaskAsync(url);
                    ProtocolConfig config = null;

                    // Try plain JSON first
                    try
                    {
                        config = JsonConvert.DeserializeObject<ProtocolConfig>(fileContent);
                    }
                    catch
                    {
                        // Try decrypt
                        string password = PromptPassword("Enter password to unlock cloud config:");
                        string decrypted = ConfigEncryption.Decrypt(fileContent, password);
                        config = JsonConvert.DeserializeObject<ProtocolConfig>(decrypted);
                    }

                    if (config == null || string.IsNullOrEmpty(config.Protocol))
                    {
                        LogMessage("Invalid or corrupted config from cloud.");
                        return;
                    }

                    // Set protocol
                    foreach (ComboBoxItem item in ProtocolComboBox.Items)
                    {
                        if (item.Content.ToString() == config.Protocol)
                        {
                            ProtocolComboBox.SelectedItem = item;
                            break;
                        }
                    }

                    ApplyConfig(config);
                    LogMessage("Cloud config imported successfully.");
                }
            }
            catch (Exception ex)
            {
                LogMessage("Failed to import config from cloud: " + ex.Message);
            }
        }
        private string PromptInput(string title)
        {
            var inputDialog = new InputDialog(title); // We'll create this next
            if (inputDialog.ShowDialog() == true)
                return inputDialog.TextInput;

            return "";
        }






    }
}

// Configuration class for all protocol settings
public class ProtocolConfig
{
    public string Protocol { get; set; }
    public string SSHHost { get; set; }
    public string SSHPort { get; set; }
    public string SSHUsername { get; set; }
    public string SSHPassword { get; set; }
    public string WireGuardConfigPath { get; set; }
    public string OpenVPNConfigPath { get; set; }
    public string V2RayConfigPath { get; set; }
    public string ShadowSocksConfigPath { get; set; }
    public bool IsLocked { get; set; }  // ✅ Add this

}




