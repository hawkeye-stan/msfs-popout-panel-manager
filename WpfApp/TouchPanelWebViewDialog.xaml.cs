using MahApps.Metro.Controls;
using Microsoft.Web.WebView2.Core;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace MSFSPopoutPanelManager.WpfApp
{
    public partial class TouchPanelWebViewDialog : MetroWindow
    {
        private string _planeId;
        private string _panelId;
        private CoreWebView2Environment _webView2Environment;

        public TouchPanelWebViewDialog(string planeId, string panelId, string caption, int width, int height, CoreWebView2Environment environment)
        {
            InitializeComponent();
            //this.Topmost = true;
            this.Title = caption;
            //this.Width = width;
            //this.Height = height;

            _planeId = planeId;
            _panelId = panelId;
            _webView2Environment = environment;

            Loaded += TouchPanelWebViewDialog_Loaded;
        }

        private async void TouchPanelWebViewDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await webView.EnsureCoreWebView2Async(_webView2Environment);

            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate($"{Constants.WEB_HOST_URI}/{_planeId.ToLower()}/{_panelId.ToLower()}");
            }

            // This fixes webview which does not maximize correctly when host WPF dialog is maximized
            WindowExtensions.FixWindowMaximizeCropping(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            webView.Dispose();
        }
    }

    internal static class WindowExtensions
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowLong(IntPtr hwnd, int nIndex);

        [DllImport("USER32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        private const int GWL_STYLE = -16;
        private const uint WS_MAXIMIZEBOX = 0x10000;
        private const uint WS_MINIMIZEBOX = 0x20000;

        public static void FixWindowMaximizeCropping(this Window window)
        {
            window.SourceInitialized += (s, e) =>
            {
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
                var currentStyle = GetWindowLong(hwnd, GWL_STYLE).ToInt64();

                SetWindowLong(hwnd, GWL_STYLE, (uint)(currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
            };
        }
    }
}
