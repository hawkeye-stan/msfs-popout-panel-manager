using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.AppUserControl
{
    public partial class ProfileCard
    {
        private readonly ProfileCardViewModel _viewModel;

        public ProfileCard()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            _viewModel = App.AppHost.Services.GetRequiredService<ProfileCardViewModel>();
            Loaded += (_, _) => { DataContext = _viewModel; };

#if LOCAL || DEBUG
            this.WrapPanelSwitchWindow.Visibility = Visibility.Visible;
#else
            this.WrapPanelSwitchWindow.Visibility = Visibility.Collapsed;
#endif
        }

        private void ToggleButtonEditProfileTitle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton { IsChecked: not null } toggleButton && (bool)toggleButton.IsChecked)
            {
                TxtBoxProfileTitle.Dispatcher.BeginInvoke(new Action(() => TxtBoxProfileTitle.Focus()));
                TxtBoxProfileTitle.Dispatcher.BeginInvoke(new Action(() => TxtBoxProfileTitle.SelectAll()));
            }
        }

        private void TxtBoxProfileTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) 
                return;

            ToggleButtonEditProfileTitle.IsChecked = false;
            Keyboard.ClearFocus();
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(RootCard), RootCard);
        }

        private void IncludeInGamePanel_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            _viewModel.IncludeInGamePanelUpdatedCommand?.Execute(null);
        }

        private void AddHudBar_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            _viewModel.AddHudBarUpdatedCommand?.Execute(null);
        }

        private void AddRefocusDisplay_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            _viewModel.RefocusDisplayUpdatedCommand?.Execute(null);
        }

        private void AddNumPad_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            _viewModel.AddNumPadUpdatedCommand?.Execute(null);
        }

        private void AddSwitchWindow_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            _viewModel.AddSwitchWindowUpdatedCommand?.Execute(null);
        }
    }


    [ValueConversion(typeof(DateTime), typeof(String))]
    public class StringToHudBarTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString()?.Replace("_", " ");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var data = value.ToString();
            return Enum.Parse<HudBarType>(data.Replace(" ", "_"));
        }
    }
}
