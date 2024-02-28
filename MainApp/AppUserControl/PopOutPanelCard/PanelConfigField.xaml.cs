using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.AppUserControl.PopOutPanelCard
{
    public partial class PanelConfigField
    {
        private readonly PanelConfigFieldViewModel _viewModel;

        public static readonly DependencyProperty DataItemProperty = DependencyProperty.Register(nameof(DataItem), typeof(PanelConfig), typeof(PanelConfigField));
        public static readonly DependencyProperty BindingPathProperty = DependencyProperty.Register(nameof(BindingPath), typeof(string), typeof(PanelConfigField));
        public static readonly RoutedEvent SourceUpdatedEvent = EventManager.RegisterRoutedEvent("SourceUpdatedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PanelConfigField));

        public PanelConfigField()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                InitializeComponent();
                return;
            }

            _viewModel = App.AppHost.Services.GetRequiredService<PanelConfigFieldViewModel>();
            Loaded += (_, _) =>
            {
                _viewModel.DataItem = DataItem;
                _viewModel.BindingPath = BindingPath;
                _viewModel.SourceUpdatedEvent = SourceUpdatedEvent;
                this.DataContext = _viewModel;
                InitializeComponent();

                var binding = new Binding($"DataItem.{BindingPath}")
                {
                    Mode = BindingMode.TwoWay,
                    NotifyOnSourceUpdated = true
                };

                TxtBoxData?.SetBinding(TextBox.TextProperty, binding);
            };
        }

        public PanelConfig DataItem
        {
            get => (PanelConfig)GetValue(DataItemProperty);
            set => SetValue(DataItemProperty, value);
        }

        public string BindingPath
        {
            get => (string)GetValue(BindingPathProperty);
            set => SetValue(BindingPathProperty, value);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) 
                return;

            Keyboard.ClearFocus();
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this.Parent), this.Parent as IInputElement);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtBoxData.Dispatcher.BeginInvoke(new Action(() => TxtBoxData.SelectAll()));
        }

        private string _oldText;

        private void TxtBox_NumbersOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(int.TryParse(e.Text, out _) || (e.Text.Trim() == "-"));

            if (!e.Handled)
                _oldText = ((TextBox)sender).Text;
        }

        private void TxtBox_NumbersOnlyTextChanged(object sender, TextChangedEventArgs e)
        {
            var txtBox = (TextBox)sender;

            if (String.IsNullOrEmpty(txtBox.Text))
                txtBox.Text = "0";

            else if (!(int.TryParse(txtBox.Text, out _) || (txtBox.Text.Trim() == "-")))
            {
                txtBox.Text = _oldText;
            }
        }

        private void Data_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            _viewModel.DataUpdatedCommand.Execute(null);
        }

        private void BtnPopupBoxOpen_Click(object sender, RoutedEventArgs e)
        {
            PopupBoxAdjustment.IsPopupOpen = true;
        }

        private void BtnPopupBoxClose_Click(object sender, RoutedEventArgs e)
        {
            PopupBoxAdjustment.IsPopupOpen = false;
        }
    }
}
