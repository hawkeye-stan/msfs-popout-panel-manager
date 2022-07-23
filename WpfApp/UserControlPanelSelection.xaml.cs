using MSFSPopoutPanelManager.WpfApp.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace MSFSPopoutPanelManager.WpfApp
{
    public partial class UserControlPanelSelection : UserControl
    {
        private PanelSelectionViewModel _panelSelectionViewModel;

        public UserControlPanelSelection(PanelSelectionViewModel panelSelectionViewModel)
        {
            InitializeComponent();
            _panelSelectionViewModel = panelSelectionViewModel;
            this.DataContext = panelSelectionViewModel;

            panelSelectionViewModel.OpenTouchPanelBindingDialog += HandleOpenTouchPanelBindingDialog;
        }

        private void HandleOpenTouchPanelBindingDialog(object sender, System.EventArgs e)
        {
            TouchPanelBindingDialog dialog = new TouchPanelBindingDialog(_panelSelectionViewModel.TouchPanelBindingViewModel);
            _panelSelectionViewModel.TouchPanelBindingViewModel.Initialize();
            dialog.Owner = Application.Current.MainWindow;
            dialog.Topmost = true;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }
    }
}
