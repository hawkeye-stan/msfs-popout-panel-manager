using MSFSPopoutPanelManager.WpfApp.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace MSFSPopoutPanelManager.WpfApp
{
    /// <summary>
    /// Interaction logic for UserControlPanelSelection.xaml
    /// </summary>
    public partial class UserControlPanelSelection : UserControl
    {
        private PanelSelectionViewModel _panelSelectionViewModel;

        public UserControlPanelSelection(PanelSelectionViewModel panelSelectionViewModel)
        {
            InitializeComponent();
            _panelSelectionViewModel = panelSelectionViewModel;
            this.DataContext = panelSelectionViewModel;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            cmbProfile.SelectionChanged -= Profile_Changed;
            _panelSelectionViewModel.Initialize();
            cmbProfile.SelectionChanged += Profile_Changed;
        }

        private void AddProfile_Click(object sender, RoutedEventArgs e)
        {
            AddProfileDialog dialog = new AddProfileDialog(_panelSelectionViewModel.DataStore.UserProfiles);
            dialog.Owner = Application.Current.MainWindow;
            dialog.Topmost = true;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if ((bool)dialog.ShowDialog())
            {
                _panelSelectionViewModel.AddProfileCommand.Execute(new AddProfileCommandParameter() { ProfileName = dialog.ProfileName, CopyProfileId = dialog.SelectedCopyProfileId });
            }
        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Confirm Delete", "Are you sure you want to delete the selected profile?");
            dialog.Owner = Application.Current.MainWindow;
            dialog.Topmost = true;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if ((bool)dialog.ShowDialog())
            {
                _panelSelectionViewModel.DeleteProfileCommand.Execute(null);
            }
        }

        private void Profile_Changed(object sender, RoutedEventArgs e)
        {
            _panelSelectionViewModel.ChangeProfileCommand.Execute(null);
        }

        private void StartPanelSelection_Click(object sender, RoutedEventArgs e)
        {
            if (_panelSelectionViewModel.DataStore.ActiveUserProfile.PanelSourceCoordinates.Count > 0)
            {
                ConfirmationDialog dialog = new ConfirmationDialog("Confirm Overwrite", "WARNING! Are you sure you want to overwrite existing saved panel locations and all saved setttings for this profile?");
                dialog.Owner = Application.Current.MainWindow;
                dialog.Topmost = true;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                if ((bool)dialog.ShowDialog())
                {
                    _panelSelectionViewModel.StartPanelSelectionCommand.Execute(null);
                }
            }
            else
            {
                _panelSelectionViewModel.StartPanelSelectionCommand.Execute(null);
            }
        }

        private void SaveAutoPanningCamera_Click(object sender, RoutedEventArgs e)
        {
            if (_panelSelectionViewModel.DataStore.ActiveUserProfile.PanelSourceCoordinates.Count > 0)
            {
                ConfirmationDialog dialog = new ConfirmationDialog("Confirm Overwrite Auto Panning Camera", "WARNING! Are you sure you want to overwrite existing Auto Panning camera angle?");
                dialog.Owner = Application.Current.MainWindow;
                dialog.Topmost = true;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                if ((bool)dialog.ShowDialog())
                {
                    _panelSelectionViewModel.SaveAutoPanningCameraCommand.Execute(null);
                }
            }
        }

        private void AddBinding_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Confirm Add Binding", $"Are you sure you want to bind the aircraft livery below to the active profile? \n{_panelSelectionViewModel.DataStore.CurrentMsfsPlaneTitle}");
            dialog.Owner = Application.Current.MainWindow;
            dialog.Topmost = true;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if ((bool)dialog.ShowDialog())
            {
                _panelSelectionViewModel.AddProfileBindingCommand.Execute(null);
            }
        }

        private void DeleteBinding_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Confirm Delete Binding", $"Are you sure you want to delete aircraft livery binding below from the active profile? \n{_panelSelectionViewModel.DataStore.CurrentMsfsPlaneTitle}");
            dialog.Owner = Application.Current.MainWindow;
            dialog.Topmost = true;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if ((bool)dialog.ShowDialog())
            {
                _panelSelectionViewModel.DeleteProfileBindingCommand.Execute(null);
            }
        }
    }
}
