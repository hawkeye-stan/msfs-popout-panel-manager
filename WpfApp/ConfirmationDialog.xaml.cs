using MahApps.Metro.Controls;
using System.Windows;

namespace MSFSPopoutPanelManager.WpfApp
{
    public partial class ConfirmationDialog : MetroWindow
    {
        public ConfirmationDialog(string title, string message)
        {
            InitializeComponent();
            this.Title = title;
            this.txtMessage.Text = message;
        }

        private void btnDialogYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
