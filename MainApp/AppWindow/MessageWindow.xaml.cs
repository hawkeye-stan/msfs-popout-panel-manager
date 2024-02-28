using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Interop;
using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;

namespace MSFSPopoutPanelManager.MainApp.AppWindow
{
    public partial class MessageWindow
    {
        private readonly MessageWindowViewModel _viewModel;

        public MessageWindow()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            _viewModel = App.AppHost.Services.GetRequiredService<MessageWindowViewModel>();
            Loaded += (_, _) =>
            {
                DataContext = _viewModel;

                var window = Window.GetWindow(this);
                if (window == null)
                    throw new ApplicationException("Unable to instantiate status message window");
                
                _viewModel.Handle = new WindowInteropHelper(window).Handle;

                // Set window binding, needs to be in code after window loaded
                var visibleBinding = new Binding("IsVisible")
                {
                    Source = _viewModel,
                    Converter = new BooleanToVisibilityConverter()
                };
                BindingOperations.SetBinding(this, Window.VisibilityProperty, visibleBinding);

                // Set window click through
                WindowsServices.SetWindowExTransparent(_viewModel.Handle);

                _viewModel.OnMessageUpdated += ViewModel_OnMessageUpdated;
            };
        }

        private void ViewModel_OnMessageUpdated(object sender, List<Run> e)
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
        static extern int GetWindowLong(IntPtr hWnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int index, int newStyle);

        public static void SetWindowExTransparent(IntPtr hWnd)
        {
            var extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
    }
}
