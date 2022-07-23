using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro.Controls;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using MSFSPopoutPanelManager.WpfApp.ViewModel;
using Prism.Commands;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;

namespace MSFSPopoutPanelManager.WpfApp
{
    public partial class ApplicationWindow : MetroWindow
    {
        private ApplicationViewModel _viewModel;

        // This command has to be here since it doesn't work in view model, window StateChanged never gets fire
        public DelegateCommand RestoreWindowCommand => new DelegateCommand(() => { this.WindowState = WindowState.Normal; }, () => { return true; });

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

            WindowActionManager.OnPopOutManagerAlwaysOnTopChanged += (sender, e) => { this.Topmost = e; };
        }

        private void HandleShowContextMenuBalloonTip(object sender, StatusMessageEventArg e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                notifyIcon.ShowBalloonTip("MSFS Pop Out Panel Manager Error", e.Message, BalloonIcon.Error);
            });
        }

        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    this.ShowInTaskbar = true;
                    break;
                case WindowState.Minimized:
                    if (_viewModel.AppSettingData.AppSetting.MinimizeToTray)
                    {
                        notifyIcon.Visibility = Visibility.Visible;
                        this.ShowInTaskbar = false;
                    }
                    break;
                case WindowState.Normal:
                    notifyIcon.Visibility = Visibility.Hidden;
                    this.ShowInTaskbar = true;

                    // Fix always on top status once app is minimize and then restore
                    if (_viewModel.AppSettingData.AppSetting.AlwaysOnTop)
                        WindowActionManager.ApplyAlwaysOnTop(_viewModel.ApplicationHandle, PanelType.PopOutManager, true);
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.ApplicationHandle = new WindowInteropHelper(Window.GetWindow(this)).Handle;
            _viewModel.ApplicationWindow = Application.Current.MainWindow;
            _viewModel.Initialize();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            await _viewModel.WindowClosing();
        }
    }
}
