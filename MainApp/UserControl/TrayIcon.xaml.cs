using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MSFSPopoutPanelManager.MainApp
{
    public partial class TrayIcon : UserControl
    {
        // This command has to be here since it doesn't work in view model, window StateChanged never gets fire
        public DelegateCommand RestoreWindowCommand => new DelegateCommand(() => { ((Window)((Border)((Grid)this.Parent).Parent).Parent).WindowState = WindowState.Normal; }, () => { return true; });

        private TrayIconViewModel ViewModel { get; set; }

        public TrayIcon()
        {
            ViewModel = App.AppHost.Services.GetRequiredService<TrayIconViewModel>();
            InitializeComponent();
            Loaded += (sender, e) => { DataContext = ViewModel; };

            Tray.DoubleClickCommand = RestoreWindowCommand;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Tray.ContextMenu.IsOpen = true;
        }
    }
}
