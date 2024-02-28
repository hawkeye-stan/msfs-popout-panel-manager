using MSFSPopoutPanelManager.MainApp.ViewModel;
using System.ComponentModel;
using System.Windows;

namespace MSFSPopoutPanelManager.MainApp.AppUserControl.Dialog
{
    public partial class ConfirmationDialog
    {
        public ConfirmationDialog(string content, string confirmButtonText)
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            DataContext = new ConfirmationViewModel(content, confirmButtonText);
        }
    }
}
