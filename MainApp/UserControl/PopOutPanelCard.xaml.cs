using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp
{
    public partial class PopOutPanelCard : UserControl
    {
        private PopOutPanelCardViewModel _viewModel;
        public static readonly DependencyProperty DataItemProperty = DependencyProperty.Register("DataItem", typeof(PanelConfig), typeof(PopOutPanelCard));

        public PopOutPanelCard()
        {
            _viewModel = App.AppHost.Services.GetRequiredService<PopOutPanelCardViewModel>();
            Loaded += (sender, e) =>
            {
                _viewModel.DataItem = DataItem;
                this.DataContext = _viewModel;
                InitializeComponent();
            };
        }

        public PanelConfig DataItem
        {
            get { return (PanelConfig)GetValue(DataItemProperty); }
            set { SetValue(DataItemProperty, value); }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(RootExpander), RootExpander as IInputElement);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var txtBox = (TextBox)sender;
            txtBox.Dispatcher.BeginInvoke(new Action(() => txtBox.SelectAll()));
        }

        private void BtnUpDown_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var buttonType = button.Name.Substring(9);

            var txtBox = this.FindName($"TxtBox{buttonType}") as TextBox;
            if (txtBox != null)
                txtBox.Dispatcher.BeginInvoke(new Action(() => txtBox.Focus()));
        }

        private string _oldText;

        private void TxtBox_NumbersOnly(object sender, TextCompositionEventArgs e)
        {
            int result;
            e.Handled = !(int.TryParse(e.Text, out result) || (e.Text.Trim() == "-"));

            if (!e.Handled)
                _oldText = ((TextBox)sender).Text;
        }

        private void TxtBox_NumbersOnlyTextChanged(object sender, TextChangedEventArgs e)
        {
            int result;
            var txtBox = (TextBox)sender;

            if (String.IsNullOrEmpty(txtBox.Text))
                txtBox.Text = "0";
            else if (!(int.TryParse(txtBox.Text, out result) || (txtBox.Text.Trim() == "-")))
            {
                txtBox.Text = _oldText;
            }
        }

        private void Data_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            string? param = null;

            if (sender is PanelConfigField)
                param = ((PanelConfigField)sender).BindingPath;
            else if (sender is ToggleButton)
                param = ((ToggleButton)sender).Name.Substring(6);
            else if (sender is TextBox)
                param = ((TextBox)sender).Name.Substring(6);

            if (!String.IsNullOrEmpty(param))
                _viewModel.PanelAttributeUpdatedCommand.Execute(param);
        }

        private void PanelSourceIcon_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _viewModel.DataItem != null && _viewModel.ProfileData.ActiveProfile.IsEditingPanelSource)
                _viewModel.DataItem.IsShownPanelSource = true;
        }

        private void PanelSourceIcon_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released && _viewModel.DataItem != null && _viewModel.ProfileData.ActiveProfile.IsEditingPanelSource)
                _viewModel.DataItem.IsShownPanelSource = false;
        }
    }

    public class CustomTextBox : TextBox
    {
        static CustomTextBox()
        {
            TextProperty.OverrideMetadata(typeof(CustomTextBox), new FrameworkPropertyMetadata(null, null, CoerceChanged));
        }

        private static object CoerceChanged(DependencyObject d, object basevalue)
        {
            TextBox? txtBox = d as TextBox;
            if (txtBox != null && basevalue == null)
            {
                return txtBox.Text;
            }
            return basevalue;
        }
    }
}
