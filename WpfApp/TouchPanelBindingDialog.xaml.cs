using MahApps.Metro.Controls;
using MSFSPopoutPanelManager.WpfApp.ViewModel;

namespace MSFSPopoutPanelManager.WpfApp
{
    public partial class TouchPanelBindingDialog : MetroWindow
    {
        public TouchPanelBindingDialog(TouchPanelBindingViewModel _touchPanelBindingViewModel)
        {
            InitializeComponent();
            this.DataContext = _touchPanelBindingViewModel;
        }
    }
}
