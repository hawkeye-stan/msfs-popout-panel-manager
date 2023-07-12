using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MSFSPopoutPanelManager.MainApp
{
    public partial class HelpDrawer : UserControl
    {
        private HelpViewModel _viewModel;

        public HelpDrawer()
        {
            _viewModel = App.AppHost.Services.GetRequiredService<HelpViewModel>();
            InitializeComponent();
            Loaded += (sender, e) => { DataContext = _viewModel; };
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            _viewModel.HyperLinkCommand.Execute(e.Uri.ToString());
        }
    }
}
