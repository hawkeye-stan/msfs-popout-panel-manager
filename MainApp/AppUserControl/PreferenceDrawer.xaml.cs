using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System.ComponentModel;
using System.Windows;

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
    }
}