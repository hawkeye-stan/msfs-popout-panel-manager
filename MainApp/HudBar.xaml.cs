using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace MSFSPopoutPanelManager.MainApp
{
    public partial class HudBar : Window
    {
        private HudBarViewModel _viewModel;

        public HudBar(Guid panelId)
        {
            _viewModel = App.AppHost.Services.GetRequiredService<HudBarViewModel>();
            _viewModel.PanelId = panelId;

            InitializeComponent();
            Loaded += (sender, e) =>
            {
                DataContext = _viewModel;
                _viewModel.PanelConfig.PanelHandle = new WindowInteropHelper(Window.GetWindow(this)).Handle;
                _viewModel.PanelConfig.Width = Convert.ToInt32(Width);
                _viewModel.PanelConfig.Height = Convert.ToInt32(Height);
            };

            this.MouseLeftButtonDown += HudBar_MouseLeftButtonDown;
            this.Topmost = true;

            Closing += HudBar_Closing;
        }

        private void HudBar_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
