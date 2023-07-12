using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System;
using System.Windows.Controls;

namespace MSFSPopoutPanelManager.MainApp
{
    public partial class AddProfileDialog : UserControl
    {
        public AddProfileViewModel ViewModel { get; set; }

        public AddProfileDialog()
        {
            ViewModel = App.AppHost.Services.GetRequiredService<AddProfileViewModel>();
            InitializeComponent();
            Loaded += (sender, e) =>
            {
                DataContext = ViewModel;
                BtnAccept.IsEnabled = false;
            };
        }

        public DialogClosingEventHandler ClosingEventHandler { get { return ViewModel.ClosingEventHandler; } }
    }
}
