using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.UserDataAgent;
using MSFSPopoutPanelManager.WpfApp.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MSFSPopoutPanelManager.WpfApp
{
    public partial class UserControlPanelConfiguration : UserControl
    {
        private PanelConfigurationViewModel _panelConfigurationViewModel;

        public UserControlPanelConfiguration(PanelConfigurationViewModel panelConfigurationViewModel)
        {
            InitializeComponent();
            _panelConfigurationViewModel = panelConfigurationViewModel;
            this.DataContext = _panelConfigurationViewModel;
            FocusManager.SetIsFocusScope(this, true);

            PanelConfigGrid.PreviewKeyDown += PanelConfigGrid_KeyDown;
        }

        private void GridData_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            var container = VisualTreeHelper.GetParent((Control)sender) as ContentPresenter;
            var panelConfig = container.Content as PanelConfig;
            var propertyName = (PanelConfigPropertyName)Enum.Parse(typeof(PanelConfigPropertyName), ((Control)sender).Name);

            var panelConfigItem = new PanelConfigItem() { PanelIndex = panelConfig.PanelIndex, PanelConfigProperty = propertyName };
            _panelConfigurationViewModel.PanelConfigUpdatedCommand.Execute(panelConfigItem);
        }

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(DataGridCell))
            {
                var cell = sender as DataGridCell;
                var panelConfig = cell.DataContext as PanelConfig;

                PanelConfigPropertyName selectedProperty = PanelConfigPropertyName.Invalid;

                switch (cell.Column.Header)
                {
                    case "Panel Name":
                        selectedProperty = PanelConfigPropertyName.PanelName;
                        break;
                    case "X-Pos":
                        selectedProperty = PanelConfigPropertyName.Left;
                        break;
                    case "Y-Pos":
                        selectedProperty = PanelConfigPropertyName.Top;
                        break;
                    case "Width":
                        selectedProperty = PanelConfigPropertyName.Width;
                        break;
                    case "Height":
                        selectedProperty = PanelConfigPropertyName.Height;
                        break;
                    case "Always on Top":
                        selectedProperty = PanelConfigPropertyName.AlwaysOnTop;
                        break;
                    case "Hide Titlebar":
                        selectedProperty = PanelConfigPropertyName.HideTitlebar;
                        break;
                    case "Full Screen Mode":
                        selectedProperty = PanelConfigPropertyName.FullScreen;
                        break;
                    case "Touch Enabled":
                        selectedProperty = PanelConfigPropertyName.TouchEnabled;
                        break;
                    case "Disable Game Refocus":
                        selectedProperty = PanelConfigPropertyName.DisableGameRefocus;
                        break;
                    case "":
                        selectedProperty = PanelConfigPropertyName.RowHeader;
                        break;
                }

                _panelConfigurationViewModel.SelectedPanelConfigItem = new PanelConfigItem() { PanelIndex = panelConfig.PanelIndex, PanelConfigProperty = selectedProperty };
            }
        }

        private void NumericDataPoint_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = e.Source as TextBox;
            switch (textBox.Name)
            {
                case "Top":
                case "Left":
                case "Width":
                case "Height":
                    _panelConfigurationViewModel.NumericDataPointTextBox = textBox;

                    var panelConfig = textBox.DataContext as PanelConfig;
                    _panelConfigurationViewModel.SelectedPanelConfigItem = new PanelConfigItem() { PanelIndex = panelConfig.PanelIndex, PanelConfigProperty = (PanelConfigPropertyName)Enum.Parse(typeof(PanelConfigPropertyName), textBox.Name) };
                    return;
            }

            _panelConfigurationViewModel.NumericDataPointTextBox = null;
        }

        private void NumericDataPoint_LostFocus(object sender, RoutedEventArgs e)
        {
            IInputElement focusedControl = FocusManager.GetFocusedElement(this);

            if (focusedControl is Button)
            {
                var button = focusedControl as Button;
                switch (button.Name)
                {
                    case "MinusTenButton":
                    case "MinusOneButton":
                    case "PlusOneButton":
                    case "PlusTenButton":
                        var textBox = e.OriginalSource as TextBox;
                        _panelConfigurationViewModel.NumericDataPointTextBox = textBox;
                        return;
                }
            }

            _panelConfigurationViewModel.NumericDataPointTextBox = null;
        }

        private void PanelConfigGrid_KeyDown(object sender, KeyEventArgs e)
        {
            var isControlKeyHeld = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            var isShiftKeyHeld = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

            switch (_panelConfigurationViewModel.SelectedPanelConfigItem.PanelConfigProperty)
            {
                case PanelConfigPropertyName.RowHeader:
                    var multiplier = isShiftKeyHeld ? 1 : 10;
                    switch (e.Key)
                    {
                        case Key.Left:
                            _panelConfigurationViewModel.ConfigurePanelCommand.Execute(new KeyAction(isControlKeyHeld ? "Control-Left" : "Left", multiplier));
                            break;
                        case Key.Right:
                            _panelConfigurationViewModel.ConfigurePanelCommand.Execute(new KeyAction(isControlKeyHeld ? "Control-Right" : "Right", multiplier));
                            break;
                        case Key.Up:
                            _panelConfigurationViewModel.ConfigurePanelCommand.Execute(new KeyAction(isControlKeyHeld ? "Control-Up" : "Up", multiplier));
                            break;
                        case Key.Down:
                            _panelConfigurationViewModel.ConfigurePanelCommand.Execute(new KeyAction(isControlKeyHeld ? "Control-Down" : "Down", multiplier));
                            break;
                    }

                    break;
                case PanelConfigPropertyName.Top:
                case PanelConfigPropertyName.Left:
                case PanelConfigPropertyName.Height:
                case PanelConfigPropertyName.Width:
                    switch (e.Key)
                    {
                        case Key.Up:
                            if (isShiftKeyHeld)
                                _panelConfigurationViewModel.ConfigurePanelPixelCommand.Execute(1);
                            else
                                _panelConfigurationViewModel.ConfigurePanelPixelCommand.Execute(10);
                            break;
                        case Key.Down:
                            if (isShiftKeyHeld)
                                _panelConfigurationViewModel.ConfigurePanelPixelCommand.Execute(-1);
                            else
                                _panelConfigurationViewModel.ConfigurePanelPixelCommand.Execute(-10);
                            break;
                    }

                    break;
            }

        }
    }
}
