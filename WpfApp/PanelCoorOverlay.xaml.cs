using MSFSPopoutPanelManager.Shared;
using System;
using System.Windows;

namespace MSFSPopoutPanelManager.WpfApp
{
    public partial class PanelCoorOverlay : Window
    {
        private const int TOP_ADJUSTMENT = 23;      // half of window height
        private const int LEFT_ADJUSTMENT = 27;     // half of window width

        public bool IsEditingPanelLocation { get; set; }

        public IntPtr WindowHandle { get; set; }

        public event EventHandler<EventArgs<System.Drawing.Point>> WindowLocationChanged;

        public PanelCoorOverlay(int panelIndex)
        {
            InitializeComponent();
            this.lblPanelIndex.Content = panelIndex;
            IsEditingPanelLocation = false;

            this.LocationChanged += PanelCoorOverlay_LocationChanged;
        }

        public void MoveWindow(int x, int y)
        {
            this.Left = x - LEFT_ADJUSTMENT;
            this.Top = y - TOP_ADJUSTMENT;
        }

        private void PanelCoorOverlay_LocationChanged(object sender, EventArgs e)
        {
            if (this.Top is double.NaN || this.Left is double.NaN)
                return;

            var top = Convert.ToInt32(this.Top);
            var left = Convert.ToInt32(this.Left);

            WindowLocationChanged?.Invoke(this, new EventArgs<System.Drawing.Point>(new System.Drawing.Point(left + LEFT_ADJUSTMENT, top + TOP_ADJUSTMENT)));
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsEditingPanelLocation && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
