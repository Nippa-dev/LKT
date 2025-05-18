using System.Windows;

namespace LKtunnel
{
    public partial class InputDialog : Window
    {
        public string TextInput { get; private set; }

        public InputDialog(string prompt)
        {
            InitializeComponent();
            PromptText.Text = prompt;
            InputBox.Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            TextInput = InputBox.Text;
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
