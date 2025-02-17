using Renci.SshNet;
using System.Windows;
using System.Windows.Controls;

namespace LKtunnel
{
    public partial class SSH : UserControl
    {
        private SshClient sshClient;

        public SSH()
        {
            InitializeComponent();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            string host = SSHHost.Text;
            int port = int.Parse(SSHPort.Text);
            string username = SSHUsername.Text;
            string password = SSHPassword.Password;

            sshClient = new SshClient(host, port, username, password);

            try
            {
                sshClient.Connect();
                MessageBox.Show("SSH Connected!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("SSH Connection Failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            sshClient?.Disconnect();
            MessageBox.Show("SSH Disconnected!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
