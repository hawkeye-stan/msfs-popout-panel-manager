using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace MSFSPopoutPanelManager.MainApp.AppUserControl
{
    public partial class PreferenceDrawer
    {
        public PreferenceDrawer()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                InitializeComponent();
                return;
            }

            var viewModel = App.AppHost.Services.GetRequiredService<PreferenceDrawerViewModel>();
            Loaded += (_, _) =>
            {
                DataContext = viewModel;
                InitializeComponent();
            };
        }

        private void Hyperlink_OpenDataFolder(object sender, RequestNavigateEventArgs e)
        {
            // ToDo: check for folder existence
            Process.Start("explorer.exe",e.Uri.ToString());
        }
    }
}