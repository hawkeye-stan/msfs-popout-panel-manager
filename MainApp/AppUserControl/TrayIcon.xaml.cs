using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using Prism.Commands;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MSFSPopoutPanelManager.MainApp.AppUserControl
{
    public partial class TrayIcon
    {
        // This command has to be here since it doesn't work in view model, window StateChanged never gets fire
        public DelegateCommand RestoreWindowCommand => new(() => { ((Window)((Border)((Grid)this.Parent).Parent).Parent).WindowState = WindowState.Normal; }, () => true);

        private TrayIconViewModel ViewModel { get; }

        public TrayIcon()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            ViewModel = App.AppHost.Services.GetRequiredService<TrayIconViewModel>();
            Loaded += (_, _) => { DataContext = ViewModel; };

            Tray.DoubleClickCommand = RestoreWindowCommand;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Tray.ContextMenu != null)
                Tray.ContextMenu.IsOpen = true;
        }
    }
}
