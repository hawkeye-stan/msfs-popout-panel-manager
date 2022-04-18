using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro.Controls;
using MSFSPopoutPanelManager.Provider;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WpfApp.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;

namespace MSFSPopoutPanelManager.WpfApp
{
    /// <summary>
    /// Interaction logic for ApplicationWindow.xaml
    /// </summary>
    public partial class ApplicationWindow : MetroWindow
    {
        private ApplicationViewModel _viewModel;

        // This command has to be here since it doesn't work in view model, window StateChanged never gets fire
        public DelegateCommand RestoreWindowCommand => new DelegateCommand((o) => { this.WindowState = WindowState.Normal; }, (o) => { return true; });

        public ApplicationWindow()
        {
            InitializeComponent();

            _viewModel = new ApplicationViewModel();
            _viewModel.ShowContextMenuBalloonTip += HandleShowContextMenuBalloonTip;
            this.DataContext = _viewModel;

            UserControlPanelSelection userControlPanelSelection = new UserControlPanelSelection(_viewModel.PanelSelectionViewModel);
            UserControlPanelConfiguration userControlPanelConfiguration = new UserControlPanelConfiguration(_viewModel.PanelConfigurationViewModel);

            Binding panelSelectionVisibilityBinding = new Binding()
            {
                Source = this.DataContext,
                Path = new PropertyPath("IsShownPanelSelectionScreen"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new BoolToVisibilityConverter()
            };
            BindingOperations.SetBinding(userControlPanelSelection, UserControl.VisibilityProperty, panelSelectionVisibilityBinding);

            Binding panelConfigurationVisibilityBinding = new Binding()
            {
                Source = this.DataContext,
                Path = new PropertyPath("IsShownPanelConfigurationScreen"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new BoolToVisibilityConverter()
            };
            BindingOperations.SetBinding(userControlPanelConfiguration, UserControl.VisibilityProperty, panelConfigurationVisibilityBinding);

            panelSteps.Children.Add(userControlPanelSelection);
            panelSteps.Children.Add(userControlPanelConfiguration);

            notifyIcon.DoubleClickCommand = RestoreWindowCommand;
        }

        private void HandleShowContextMenuBalloonTip(object sender, EventArgs<StatusMessage> e)
        {
            if (e.Value.MessageType == StatusMessageType.Error)
            {
                notifyIcon.ShowBalloonTip("MSFS Pop Out Panel Manager Error", e.Value.Message, BalloonIcon.Error);
            }
        }

        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    this.ShowInTaskbar = true;
                    break;
                case WindowState.Minimized:
                    if (_viewModel.DataStore.AppSetting.MinimizeToTray)
                    {
                        notifyIcon.Visibility = Visibility.Visible;
                        this.ShowInTaskbar = false;
                    }
                    break;
                case WindowState.Normal:
                    notifyIcon.Visibility = Visibility.Hidden;
                    this.ShowInTaskbar = true;

                    // Fix always on top status once app is minimize and then restore
                    if (_viewModel.DataStore.AppSetting.AlwaysOnTop)
                        WindowManager.ApplyAlwaysOnTop(_viewModel.DataStore.ApplicationHandle, true);
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.DataStore.ApplicationHandle = new WindowInteropHelper(Window.GetWindow(this)).Handle;
            _viewModel.DataStore.ApplicationWindow = this;
            _viewModel.Initialize();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.ExitCommand.Execute(null);
        }

        private void EditPreferences_Click(object sender, RoutedEventArgs e)
        {
            PreferencesDialog dialog = new PreferencesDialog(_viewModel.DataStore.AppSetting);
            dialog.Owner = Application.Current.MainWindow;
            dialog.Topmost = true;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            dialog.ShowDialog();
        }
    }
}
