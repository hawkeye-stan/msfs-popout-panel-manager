using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Interop;

namespace MSFSPopoutPanelManager.MainApp
{
    public partial class MessageWindow : Window
    {
        private MessageWindowViewModel _viewModel;

        public MessageWindow()
        {
            _viewModel = App.AppHost.Services.GetRequiredService<MessageWindowViewModel>();

            InitializeComponent();
            Loaded += (sender, e) =>
            {
                DataContext = _viewModel;
                _viewModel.Handle = new WindowInteropHelper(Window.GetWindow(this)).Handle;

                // Set window binding, needs to be in code after window loaded
                Binding visibleBinding = new Binding("IsVisible");
                visibleBinding.Source = _viewModel;
                visibleBinding.Converter = new BooleanToVisibilityConverter();
                BindingOperations.SetBinding(this, Window.VisibilityProperty, visibleBinding);

                // Set window click through
                WindowsServices.SetWindowExTransparent(_viewModel.Handle);

                _viewModel.OnMessageUpdated += _viewModel_OnMessageUpdated;
            };
        }

        private void _viewModel_OnMessageUpdated(object sender, List<Run> e)
        {
            if (e == null)
                return;

            TextBlockMessage.Inlines.Clear();

            foreach (var run in e)
                TextBlockMessage.Inlines.Add(run);

            ScrollViewerMessage.ScrollToEnd();
        }
    }

    public static class WindowsServices
    {
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
    }
}
