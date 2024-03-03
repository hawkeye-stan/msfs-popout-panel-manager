using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.MainApp.AppUserControl.PopOutPanelCard;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.AppUserControl
{
    public partial class PopOutPanelConfigCard
    {
        private readonly PopOutPanelConfigCardViewModel _viewModel;
        public static readonly DependencyProperty DataItemProperty2 = DependencyProperty.Register(nameof(DataItem), typeof(PanelConfig), typeof(PopOutPanelConfigCard));

        public PopOutPanelConfigCard()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                InitializeComponent();
                return;
            }
            
            _viewModel = App.AppHost.Services.GetRequiredService<PopOutPanelConfigCardViewModel>();
            Loaded += (_, _) =>
            {
                _viewModel.DataItem = DataItem;
                DataContext = _viewModel;
                InitializeComponent();
            };
        }
        public PanelConfig DataItem
        {
            get => (PanelConfig)GetValue(DataItemProperty2);
            set => SetValue(DataItemProperty2, value);
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
            var param = sender switch
            {
                PanelConfigField field => field.BindingPath,
                ToggleButton button => button.Name.Substring(6),
                TextBox box => box.Name.Substring(6),
                _ => null
            };

            if (!string.IsNullOrEmpty(param))
                _viewModel.PanelAttributeUpdatedCommand.Execute(param);
        }
    }
}

