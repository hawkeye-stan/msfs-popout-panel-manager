using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using MSFSPopoutPanelManager.WindowsAgent;
using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Interop;

namespace MSFSPopoutPanelManager.MainApp
{
    public partial class AppWindow : Window
    {
        private ApplicationViewModel _viewModel;

        // This command has to be here since it doesn't work in view model, window StateChanged never gets fire
        public DelegateCommand RestoreWindowCommand => new DelegateCommand(() => { this.WindowState = WindowState.Normal; }, () => { return true; });

        public AppWindow()
        {
            _viewModel = App.AppHost.Services.GetRequiredService<ApplicationViewModel>();
            InitializeComponent();
            Loaded += AppWindow_Loaded;
            Closing += AppWindow_Closing;
            StateChanged += AppWindow_StateChanged;
            WindowActionManager.OnPopOutManagerAlwaysOnTopChanged += (sender, e) => { this.Topmost = e; };
            this.MouseLeftButtonDown += (sender, e) => DragMove();
        }

        private void AppWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.ApplicationHandle = new WindowInteropHelper(Window.GetWindow(this)).Handle;
            _viewModel.ApplicationWindow = Application.Current.MainWindow;
            _viewModel.Initialize();

            this.DataContext = _viewModel;

            // Try to fix always on to click through. This won't work for app's title bar since mouseEnter won't be triggered. 
            // This is super tricky to trick POPM process is MSFS process to avoid Windows OS focus stealing
            this.MouseEnter += (sender, e) =>WindowActionManager.SetWindowFocus(WindowProcessManager.GetApplicationProcess().Handle);
        }

        private void AppWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            _viewModel.WindowClosing();
        }

        private void AppWindow_StateChanged(object? sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    this.ShowInTaskbar = true;
                    break;
                case WindowState.Minimized:
                    if (_viewModel.AppSettingData.ApplicationSetting.GeneralSetting.MinimizeToTray)
                    {
                        SystemTrayIcon.Tray.Visibility = Visibility.Visible;
                        this.ShowInTaskbar = false;
                    }
                    break;
                case WindowState.Normal:
                    SystemTrayIcon.Tray.Visibility = Visibility.Hidden;
                    this.ShowInTaskbar = true;

                    // Fix always on top status once app is minimize and then restore
                    if (_viewModel.AppSettingData.ApplicationSetting.GeneralSetting.AlwaysOnTop)
                        WindowActionManager.ApplyAlwaysOnTop(_viewModel.ApplicationHandle, PanelType.PopOutManager, true);
                    break;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.panelDrawers.Children.Clear();
            this.panelDrawers.Children.Add(new PreferenceDrawer());
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            this.panelDrawers.Children.Clear();
            this.panelDrawers.Children.Add(new HelpDrawer());
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
