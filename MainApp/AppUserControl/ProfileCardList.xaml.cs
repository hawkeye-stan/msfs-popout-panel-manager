using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System.ComponentModel;
using System.Windows;

namespace MSFSPopoutPanelManager.MainApp.AppUserControl
{
    public partial class ProfileCardList
    {
        private readonly ProfileCardListViewModel _viewModel;

        public ProfileCardList()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            _viewModel = App.AppHost.Services.GetRequiredService<ProfileCardListViewModel>();
            Loaded += (_, _) =>
            {
                DataContext = _viewModel;
                _viewModel.OnProfileSelected += (_, _) =>
                {
                    PopupBoxFinder.StaysOpen = false;
                    PopupBoxFinder.IsPopupOpen = false;
                };
            };
        }

        private void BtnPopupBoxFinder_Click(object sender, RoutedEventArgs e)
        {
            PopupBoxFinder.IsPopupOpen = !PopupBoxFinder.IsPopupOpen;
            PopupBoxFinder.StaysOpen = PopupBoxFinder.IsPopupOpen;

            if (PopupBoxFinder.IsPopupOpen)
            {
                ComboBoxSearchProfile.Text = null;
                ComboBoxSearchProfile.Focus();
            }
        }
    }
}
