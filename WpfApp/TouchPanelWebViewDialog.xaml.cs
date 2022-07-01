using MahApps.Metro.Controls;
using Microsoft.Web.WebView2.Wpf;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.WpfApp
{
    public partial class TouchPanelWebViewDialog : MetroWindow
    {
        private string _planeId;
        private string _panelId;

        public TouchPanelWebViewDialog(string planeId, string panelId, string caption, int width, int height)
        {
            InitializeComponent();
            this.Topmost = true;
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

        private async Task StartWebView(WebView2 webView)
        {
            webView.Source = new System.Uri($"http://localhost:27010/{_planeId.ToLower()}/{_panelId.ToLower()}");
        }
    }
}
