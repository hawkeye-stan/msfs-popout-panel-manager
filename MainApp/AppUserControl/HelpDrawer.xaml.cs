using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;

namespace MSFSPopoutPanelManager.MainApp.AppUserControl
{
    public partial class HelpDrawer
    {
        private readonly HelpViewModel _viewModel;

        public HelpDrawer()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
            
            _viewModel = App.AppHost.Services.GetRequiredService<HelpViewModel>();
            Loaded += (_, _) => { DataContext = _viewModel; };
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            _viewModel.HyperLinkCommand.Execute(e.Uri.ToString());
        }
    }
}
