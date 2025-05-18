using System.Windows;

namespace LKtunnel
{
    public partial class PasswordDialog : Window
    {
        public string PasswordInput { get; private set; }

        public PasswordDialog(string title)
        {
            InitializeComponent();
            this.Title = title;
            this.PromptText.Text = title;
            this.PasswordBox.Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            PasswordInput = PasswordBox.Password;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
