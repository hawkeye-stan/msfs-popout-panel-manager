using System;
using System.Windows;

namespace MSFSPopoutPanelManager.WpfApp
{
    public partial class PanelCoorOverlay : Window
    {
        public bool IsEditingPanelLocation { get; set; }

        public IntPtr WindowHandle { get; set; }

        public PanelCoorOverlay(int panelIndex)
        {
            InitializeComponent();
            this.lblPanelIndex.Content = panelIndex;
            IsEditingPanelLocation = false;
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsEditingPanelLocation && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
