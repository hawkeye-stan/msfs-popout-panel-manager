using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace MSFSPopoutPanelManager.MainApp.AppWindow
{
    /// <summary>
    /// Interaction logic for SwitchWindow.xaml
    /// </summary>
    public partial class SwitchWindow : Window
    {
        private readonly SwitchWindowViewModel _viewModel;

        public SwitchWindow(Guid panelId, int initialWidth, int initialHeight)
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            _viewModel = App.AppHost.Services.GetRequiredService<SwitchWindowViewModel>();
            _viewModel.PanelId = panelId;

            Loaded += (_, _) =>
            {
                DataContext = _viewModel;

                var window = Window.GetWindow(this);
                if (window == null)
                    throw new ApplicationException("Unable to instantiate switchWindow window");

                _viewModel.PanelConfig.PanelHandle = new WindowInteropHelper(window).Handle;

                if (initialWidth == 0 && initialHeight == 0)
                {
                    this.Width = 410;
                    this.Height = 75;
                    _viewModel.PanelConfig.Width = Convert.ToInt16(this.Width);
                    _viewModel.PanelConfig.Height = Convert.ToInt32(this.Height);
                }
                else
                {
                    this.Width = initialWidth;
                    this.Height = initialHeight;
                }
            };

            this.MouseLeftButtonDown += SwitchWindow_MouseLeftButtonDown;
            this.Topmost = true;
        }

        private void SwitchWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
