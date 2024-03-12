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
    public partial class PopOutPanelSourceCard
    {
        private readonly PopOutPanelSourceCardViewModel _viewModel;
        public static readonly DependencyProperty DataItemProperty = DependencyProperty.Register(nameof(DataItem), typeof(PanelConfig), typeof(PopOutPanelSourceCard));

        public PopOutPanelSourceCard()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                InitializeComponent();
                return;
            }
            
            _viewModel = App.AppHost.Services.GetRequiredService<PopOutPanelSourceCardViewModel>();
            Loaded += (_, _) =>
            {
                _viewModel.DataItem = DataItem;
                DataContext = _viewModel;
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

        private void RootExpander_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.DataItem.PanelType != PanelType.CustomPopout)
                return;

            var fe = e.OriginalSource as FrameworkElement;

            if (fe?.Name != "HeaderSiteContent" && fe?.Name != "BtnShowSourcePanel" && fe?.Name != "BtnIdentifySourcePanel")
                return;

            foreach (var panelConfig in _viewModel.ActiveProfile.PanelConfigs)
                panelConfig.IsSelectedPanelSource = false;

            _viewModel.EditPanelSourceCommand.Execute();
        }
    }
}
