using MahApps.Metro.Controls;
using Microsoft.Web.WebView2.Wpf;
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

        public TouchPanelWebViewDialog(string planeId, string panelId, string caption, int width, int height)
        {
            InitializeComponent();
            //this.Topmost = true;
            this.Title = caption;
            this.Width = width;
            this.Height = height;

            _planeId = planeId;
            _panelId = panelId;

            Loaded += TouchPanelWebViewDialog_Loaded;
        }

        private void TouchPanelWebViewDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            StartWebView(webView);

            // This somehow fixes webview did not maximize correctly when the host WPF dialog is maximized
            WindowExtensions.FixWindowMaximizeCropping(this);
        }

        private void StartWebView(WebView2 webView)
        {
            webView.Source = new Uri($"{Constants.WEB_HOST_URI}/{_planeId.ToLower()}/{_panelId.ToLower()}");
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
