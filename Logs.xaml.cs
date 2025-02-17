using System.Windows;
using System.Windows.Controls;

namespace LKtunnel
{
    public partial class Logs : UserControl
    {
        public Logs()
        {
            InitializeComponent();
            LoadLogs();
        }

        private void LoadLogs()
        {
            // Read logs from a file (if applicable)
            string logFilePath = "vpn_logs.txt";

            if (System.IO.File.Exists(logFilePath))
            {
                LogBox.Text = System.IO.File.ReadAllText(logFilePath);
            }
            else
            {
                LogBox.Text = "No logs available.";
            }
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            LogBox.Text = "";
            System.IO.File.WriteAllText("vpn_logs.txt", ""); // Clear log file
        }

        public void AppendLog(string message)
        {
            LogBox.AppendText($"{message}\n");
            LogBox.ScrollToEnd();
            System.IO.File.AppendAllText("vpn_logs.txt", $"{message}\n");
        }
    }
}
