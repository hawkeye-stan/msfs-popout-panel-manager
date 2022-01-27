using MSFSPopoutPanelManager.Model;
using MSFSPopoutPanelManager.WpfApp.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MSFSPopoutPanelManager.WpfApp
{
    /// <summary>
    /// Interaction logic for UserControlPanelConfiguration.xaml
    /// </summary>
    public partial class UserControlPanelConfiguration : UserControl
    {
        private PanelConfigurationViewModel _panelConfigurationViewModel;

        public UserControlPanelConfiguration(PanelConfigurationViewModel panelConfigurationViewModel)
        {
            InitializeComponent();
            _panelConfigurationViewModel = panelConfigurationViewModel;
            this.DataContext = _panelConfigurationViewModel;
        }

        private void GridData_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            var container = VisualTreeHelper.GetParent((Control) sender) as ContentPresenter;
            var panelConfig = container.Content as PanelConfig;
            var propertyName = (PanelConfigPropertyName) Enum.Parse(typeof(PanelConfigPropertyName), ((Control)sender).Name);

            var panelConfigItem = new PanelConfigItem() { PanelIndex = panelConfig.PanelIndex, PanelConfigProperty = propertyName };
            _panelConfigurationViewModel.PanelConfigUpdatedCommand.Execute(panelConfigItem);
        }

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(DataGridCell))
            {
                var cell = sender as DataGridCell;
                var panelConfig = cell.DataContext as PanelConfig;

                PanelConfigPropertyName selectedProperty = PanelConfigPropertyName.Invalid;

                switch(cell.Column.Header)
                {
                    case "Panel Name":
                        selectedProperty = PanelConfigPropertyName.PanelName;
                        break;
                    case "X-Pos":
                        selectedProperty = PanelConfigPropertyName.Left;
                        break;
                    case "Y-Pos":
                        selectedProperty = PanelConfigPropertyName.Top;
                        break;
                    case "Width":
                        selectedProperty = PanelConfigPropertyName.Width;
                        break;
                    case "Height":
                        selectedProperty = PanelConfigPropertyName.Height;
                        break;
                    case "Always on Top":
                        selectedProperty = PanelConfigPropertyName.AlwaysOnTop;
                        break;
                    case "Hide Titlebar":
                        selectedProperty = PanelConfigPropertyName.HideTitlebar;
                        break;
                }

                 _panelConfigurationViewModel.SelectedPanelConfigItem = new PanelConfigItem() { PanelIndex = panelConfig.PanelIndex, PanelConfigProperty = selectedProperty };
            }
        }

        private void LockPanels_Click(object sender, RoutedEventArgs e)
        {
            if(_panelConfigurationViewModel.DataStore.ActiveUserProfile.IsLocked)
            {
                ConfirmationDialog dialog = new ConfirmationDialog("Confirm Unlock Panels", "Are you sure you want to unlock all panels to make changes?");
                dialog.Owner = Application.Current.MainWindow;
                dialog.Topmost = true;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                if ((bool)dialog.ShowDialog())
                {
                    _panelConfigurationViewModel.LockPanelsCommand.Execute(null);
                }
            }
            else
            {
                _panelConfigurationViewModel.LockPanelsCommand.Execute(null);
            }
        }
    }
}
