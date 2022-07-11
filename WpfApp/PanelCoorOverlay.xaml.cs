using MSFSPopoutPanelManager.Provider;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Windows;
using System.Windows.Interop;

namespace MSFSPopoutPanelManager.WpfApp
{
    public partial class PanelCoorOverlay : Window
    {
        private const int TOP_ADJUSTMENT = 23;      // half of window height
        private const int LEFT_ADJUSTMENT = 27;     // half of window width

        private int _leftCoor;
        private int _topCoor;

        public bool IsEditingPanelLocation { get; set; }

        public IntPtr WindowHandle { get; set; }

        public event EventHandler<EventArgs<System.Drawing.Point>> WindowLocationChanged;

        public PanelCoorOverlay(int panelIndex)
        {
            InitializeComponent();
            this.lblPanelIndex.Content = panelIndex;
            IsEditingPanelLocation = false;

            this.LocationChanged += PanelCoorOverlay_LocationChanged;
            this.Loaded += PanelCoorOverlay_Loaded;
        }

        public void MoveWindow(int x, int y)
        {
            _leftCoor = x - LEFT_ADJUSTMENT;
            _topCoor = y - TOP_ADJUSTMENT;
        }

        private void PanelCoorOverlay_Loaded(object sender, System.EventArgs e)
        {
            // Fixed broken window left/top coordinate for DPI Awareness Per Monitor
            var handle = new WindowInteropHelper(this).Handle;
            WindowManager.MoveWindow(handle, _leftCoor, _topCoor);
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
