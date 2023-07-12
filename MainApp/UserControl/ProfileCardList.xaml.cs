using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System;
using System.Windows.Controls;

namespace MSFSPopoutPanelManager.MainApp
{
    public partial class ProfileCardList : UserControl
    {
        private ProfileCardListViewModel _viewModel;

        public ProfileCardList()
        {
            _viewModel = App.AppHost.Services.GetRequiredService<ProfileCardListViewModel>();
            InitializeComponent();
            Loaded += (sender, e) =>
            {
                DataContext = _viewModel;
                _viewModel.OnProfileSelected += (sender, e) =>
                {
                    PopupBoxFinder.StaysOpen = false;
                    PopupBoxFinder.IsPopupOpen = false;
                };
            };
        }

        private void BtnPopupBoxFinder_Click(object sender, System.Windows.RoutedEventArgs e)
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
