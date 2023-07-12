using MSFSPopoutPanelManager.MainApp.ViewModel;
using System.Windows.Controls;

namespace MSFSPopoutPanelManager.MainApp
{
    public partial class ConfirmationDialog : UserControl
    {
        public ConfirmationDialog(string content, string confirmButtonText)
        {
            InitializeComponent();
            DataContext = new ConfirmationViewModel(content, confirmButtonText);
        }
    }
}
