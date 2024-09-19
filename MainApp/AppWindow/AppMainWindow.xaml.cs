using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.MainApp.AppUserControl;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using MSFSPopoutPanelManager.WindowsAgent;

namespace MSFSPopoutPanelManager.MainApp.AppWindow
{
    public partial class AppMainWindow
    {
        private readonly ApplicationViewModel _viewModel;

        public AppMainWindow()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
            
            _viewModel = App.AppHost.Services.GetRequiredService<ApplicationViewModel>();
            Loaded += AppWindow_Loaded;
            Closing += AppWindow_Closing;
            StateChanged += AppWindow_StateChanged;
            WindowActionManager.OnPopOutManagerAlwaysOnTopChanged += (_, e) => { Topmost = e; };
            MouseLeftButtonDown += (_, _) => DragMove();

        }

        private void AppWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);

            if (window == null)
                throw new ApplicationException("Cannot instantiate Pop Out Panel Manager.");

            _viewModel.ApplicationHandle = new WindowInteropHelper(window).Handle;
            _viewModel.ApplicationWindow = Application.Current.MainWindow;
            _viewModel.Initialize();

            DataContext = _viewModel;

            // Try to fix always on to click through. This won't work for application's title bar since mouseEnter won't be triggered. 
            // This is super tricky to trick POPM process is MSFS process to avoid Windows OS focus stealing
            MouseEnter += (_, _) =>WindowActionManager.SetWindowFocus(WindowProcessManager.GetApplicationProcess().Handle);
        }

        private void AppWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            _viewModel.WindowClosing();
        }

        private void AppWindow_StateChanged(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    ShowInTaskbar = true;
                    break;
                case WindowState.Minimized:
                    if (_viewModel.AppSettingData.ApplicationSetting.GeneralSetting.MinimizeToTray)
                    {
                        SystemTrayIcon.Tray.Visibility = Visibility.Visible;
                        ShowInTaskbar = false;
                    }
                    break;
                case WindowState.Normal:
                    SystemTrayIcon.Tray.Visibility = Visibility.Hidden;
                    ShowInTaskbar = true;

                    // Fix always on top status once app is minimize and then restore
                    if (_viewModel.AppSettingData.ApplicationSetting.GeneralSetting.AlwaysOnTop)
                        WindowActionManager.ApplyAlwaysOnTop(_viewModel.ApplicationHandle, PanelType.PopOutManager, true);
                    break;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            PanelDrawers.Children.Clear();
            PanelDrawers.Children.Add(new PreferenceDrawer());
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            PanelDrawers.Children.Clear();
            PanelDrawers.Children.Add(new HelpDrawer());
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
