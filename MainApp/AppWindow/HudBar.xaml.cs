using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;

namespace MSFSPopoutPanelManager.MainApp.AppWindow
{
    public partial class HudBar
    {
        private readonly HudBarViewModel _viewModel;

        public HudBar(Guid panelId)
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            _viewModel = App.AppHost.Services.GetRequiredService<HudBarViewModel>();
            _viewModel.PanelId = panelId;
            Loaded += (_, _) =>
            {
                DataContext = _viewModel;

                var window = Window.GetWindow(this);
                if (window == null)
                    throw new ApplicationException("Unable to instantiate HudBar window");
                
                _viewModel.PanelConfig.PanelHandle = new WindowInteropHelper(window).Handle;
                _viewModel.PanelConfig.Width = Convert.ToInt32(Width);
                _viewModel.PanelConfig.Height = Convert.ToInt32(Height);
            };

            this.MouseLeftButtonDown += HudBar_MouseLeftButtonDown;
            this.Topmost = true;

            Closing += HudBar_Closing;
        }

        private void HudBar_Closing(object sender, CancelEventArgs e)
        {
            _viewModel.CloseCommand.Execute(null);
        }

        private void HudBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
