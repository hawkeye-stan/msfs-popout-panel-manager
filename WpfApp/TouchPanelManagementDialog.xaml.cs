using MahApps.Metro.Controls;
using MSFSPopoutPanelManager.WpfApp.ViewModel;

namespace MSFSPopoutPanelManager.WpfApp
{
    public partial class TouchPanelManagementDialog : MetroWindow
    {
        private TouchPanelManagementViewModel _touchPanelManagementViewModel;

        public TouchPanelManagementDialog(TouchPanelManagementViewModel touchPanelManagementViewModel)
        {
            InitializeComponent();
            _touchPanelManagementViewModel = touchPanelManagementViewModel;
            this.DataContext = _touchPanelManagementViewModel;

            txtClientLog.TextChanged += (sender, e) => { txtClientLog.ScrollToEnd(); };
            txtServerLog.TextChanged += (sender, e) => { txtServerLog.ScrollToEnd(); };
        }
    }
}
