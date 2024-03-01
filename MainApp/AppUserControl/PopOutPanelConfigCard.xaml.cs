using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.MainApp.AppUserControl.PopOutPanelCard;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using MSFSPopoutPanelManager.Orchestration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

            //if (sender is ToggleButton { Name: "TglBtnAllowFloatPanel" })
            //    ComboBoxFloatPanelKeyBinding.SelectedIndex = 0;
        }

        //private void ComboBoxFloatPanelKeyBinding_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    var comboBox = (ComboBox)sender;

        //    if (comboBox.SelectedIndex is 0 or -1)
        //    {
        //        _viewModel.DataItem.FloatingPanel.Binding = null;
        //        return;
        //    }

        //    var selectedValue = comboBox.SelectedValue.ToString();

        //    if (_viewModel.ActiveProfile.PanelConfigs.Any(x => x.FloatingPanel.Binding == selectedValue && x.Id != _viewModel.DataItem.Id))
        //        comboBox.SelectedIndex = 0;


        //    _viewModel.DataItem.FloatingPanel.Binding = selectedValue;
        //}
        
        //private readonly List<string> _floatKeyBindings = new()
        //{
        //    "",
        //    "Ctrl-1",
        //    "Ctrl-2",
        //    "Ctrl-3",
        //    "Ctrl-4",
        //    "Ctrl-5",
        //    "Ctrl-6",
        //    "Ctrl-7",
        //    "Ctrl-8",
        //    "Ctrl-9",
        //    "Ctrl-0"
        //};

        //private void ComboBoxFloatPanelKeyBinding_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    BindFloatPanelKeyBinding();
        //}

        //private void ComboBoxFloatPanelKeyBinding_OnLoaded(object sender, RoutedEventArgs e)
        //{
        //    BindFloatPanelKeyBinding();
        //}

        //private void BindFloatPanelKeyBinding()
        //{
        //    var items = new List<string>();
        //    items.AddRange(_floatKeyBindings);

        //    foreach (var panelConfig in _viewModel.ActiveProfile.PanelConfigs)
        //    {
        //        if (panelConfig.FloatingPanel.Binding != null && panelConfig.Id != _viewModel.DataItem.Id)
        //            items.Remove(panelConfig.FloatingPanel.Binding);
        //    }

        //    ComboBoxFloatPanelKeyBinding.ItemsSource = items;

        //    var index = items.ToList().FindIndex(x => string.Equals(x, _viewModel.DataItem.FloatingPanel.Binding, StringComparison.CurrentCultureIgnoreCase));

        //    if (index == -1)
        //        return;

        //    this.ComboBoxFloatPanelKeyBinding.SelectedIndex = index;
        //}

        private void BtnDetectFloatPanelKeyBinding_OnClick(object sender, RoutedEventArgs e)
        {
            this.KeyDown -= OnDetectKeyDown;
            this.KeyDown += OnDetectKeyDown;
            this.KeyUp -= OnDetectKeyUp;
            this.KeyUp += OnDetectKeyUp;

            _viewModel.DataItem.FloatingPanel.IsDetectingKeystroke = true;
        }

        private bool _isDetectingKeys;

        private void OnDetectKeyDown(object sender, KeyEventArgs e)
        {
            //if (!_isDetectingKeys)
            //{
            //    _isDetectingKeys = true;
            //    _viewModel.DataItem.FloatingPanel.Binding.Clear();
            //}
            
            //_viewModel.DataItem.FloatingPanel.Binding.Add(e.Key == Key.System ? e.SystemKey.ToString() : e.Key.ToString());
        }

        private void OnDetectKeyUp(object sender, KeyEventArgs e)
        {
            //_isDetectingKeys = false;
            //Debug.WriteLine(string.Join(" + ", _viewModel.DataItem.FloatingPanel.Binding.ToArray()));

            //this.KeyDown -= OnDetectKeyDown;
            //this.KeyUp -= OnDetectKeyUp;

            //_viewModel.DataItem.FloatingPanel.IsDetectingKeystroke = false;
        }


        //private List<KeyPressState> _capturesList = new();
        //private bool _isCapturing;

        //private GlobalKeyboardHook _globalKeyboardHook;

        //private void BtnTest_OnClick(object sender, RoutedEventArgs e)
        //{
        //    if (_globalKeyboardHook != null)
        //    {
        //        _globalKeyboardHook.KeyboardPressed -= GlobalKeyboardHookOnKeyboardPressed;
        //        _globalKeyboardHook?.Dispose();
        //        _globalKeyboardHook = null;
        //    }
        //    else
        //    {
        //        _globalKeyboardHook = new GlobalKeyboardHook();
        //        _globalKeyboardHook.KeyboardPressed += GlobalKeyboardHookOnKeyboardPressed;
        //    }
        //}

        //private void GlobalKeyboardHookOnKeyboardPressed(object sender, GlobalKeyboardHookEventArgs e)
        //{
        //    if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp)
        //    {
        //        _isCapturing = false;
        //    }

        //    if (e.KeyboardState is GlobalKeyboardHook.KeyboardState.KeyDown or GlobalKeyboardHook.KeyboardState.SysKeyDown && !_isCapturing)
        //    {
        //        _capturesList.Add(new KeyPressState { Key = e.KeyboardData.Key.ToString()});
        //        return;
        //    }
            
        //    if (e.KeyboardState is GlobalKeyboardHook.KeyboardState.KeyUp or GlobalKeyboardHook.KeyboardState.SysKeyUp && !_isCapturing)
        //    {
        //        var matched = _capturesList.FirstOrDefault(x => x.Key == e.KeyboardData.Key.ToString());

        //        if (matched == null)
        //        {
        //            // Error since there is keydown but no keyup
        //            _isCapturing = true;
        //            _capturesList.Clear();
        //        }
        //        else
        //        {
        //            matched.IsPressed = true;

        //            // Check if all keydown matches keyup
        //            if (_capturesList.All(x => x.IsPressed))
        //            {
        //                Debug.WriteLine(string.Join(" + ", _capturesList.ToArray().Select(x => x.Key)));
        //                _capturesList.Clear();
        //            }
        //        }
        //    }
        //}
    }

}

