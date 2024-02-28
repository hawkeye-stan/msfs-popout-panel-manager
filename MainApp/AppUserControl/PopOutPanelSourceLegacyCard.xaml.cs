using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.AppUserControl
{
    public partial class PopOutPanelSourceLegacyCard
    {
        private readonly PopOutPanelSourceLegacyCardViewModel _viewModel;
        public static readonly DependencyProperty DataItemProperty = DependencyProperty.Register(nameof(DataItem), typeof(PanelConfig), typeof(PopOutPanelSourceLegacyCard));

        public PopOutPanelSourceLegacyCard()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                InitializeComponent();
                return;
            }

            _viewModel = App.AppHost.Services.GetRequiredService<PopOutPanelSourceLegacyCardViewModel>();
            Loaded += (_, _) =>
            {
                _viewModel.DataItem = DataItem;
                this.DataContext = _viewModel;
                InitializeComponent();
            };
        }

        public PanelConfig DataItem
        {
            get => (PanelConfig)GetValue(DataItemProperty);
            set => SetValue(DataItemProperty, value);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e is not { Key: Key.Enter }) 
                return;

            Keyboard.ClearFocus();
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(RootExpander), RootExpander);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var txtBox = (TextBox)sender;
            txtBox.Dispatcher.BeginInvoke(new Action(() => txtBox.SelectAll()));
        }

        private void Data_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            string param = null;

            if (sender is TextBox box)
                param = box.Name.Substring(6);

            if (!string.IsNullOrEmpty(param))
                _viewModel.PanelAttributeUpdatedCommand.Execute(param);
        }
    }
}
