using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp
{
    public partial class ProfileCard : UserControl
    {
        private ProfileCardViewModel? _viewModel;

        public ProfileCard()
        {
            _viewModel = App.AppHost.Services.GetRequiredService<ProfileCardViewModel>();
            InitializeComponent();
            Loaded += (sender, e) => { DataContext = _viewModel; };
        }

        private void ToggleButtonEditProfileTitle_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton? toggleButton = sender as ToggleButton;

            if (toggleButton != null && toggleButton.IsChecked != null && (bool)toggleButton.IsChecked)
            {
                TxtBoxProfileTitle.Dispatcher.BeginInvoke(new Action(() => TxtBoxProfileTitle.Focus()));
                TxtBoxProfileTitle.Dispatcher.BeginInvoke(new Action(() => TxtBoxProfileTitle.SelectAll()));
            }
        }

        private void TxtBoxProfileTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ToggleButtonEditProfileTitle.IsChecked = false;
                Keyboard.ClearFocus();
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(rootCard), rootCard as IInputElement);
            }
        }

        private void IncludeInGamePanel_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            _viewModel.IncludeInGamePanelUpdatedCommand.Execute(null);
        }

        private void AddHudBar_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            _viewModel.AddHudBarUpdatedCommand.Execute(null);
        }

        private void AddRefocusDisplay_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            _viewModel.RefocusDisplayUpdatedCommand.Execute(null);
        }
    }


    [ValueConversion(typeof(DateTime), typeof(String))]
    public class StringToHudBarTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString().Replace("_", " ");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string data = value.ToString();
            return Enum.Parse<HudBarType>(data.Replace(" ", "_"));
        }
    }
}
