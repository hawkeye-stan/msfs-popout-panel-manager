using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System.ComponentModel;
using System.Windows;

namespace MSFSPopoutPanelManager.MainApp.AppUserControl.Dialog
{
    public partial class AddProfileDialog
    {
        public AddProfileViewModel ViewModel { get; set; }

        public AddProfileDialog()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
            
            ViewModel = App.AppHost.Services.GetRequiredService<AddProfileViewModel>();
            Loaded += (_, _) =>
            {
                DataContext = ViewModel;
                BtnAccept.IsEnabled = false;
            };
        }

        public DialogClosingEventHandler ClosingEventHandler => ViewModel.ClosingEventHandler;
    }
}
