using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;

namespace MSFSPopoutPanelManager.MainApp.AppWindow
{
    public partial class NumPad
    {
        private readonly NumPadViewModel _viewModel;

        public NumPad(Guid panelId, int initialWidth, int initialHeight)
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            _viewModel = App.AppHost.Services.GetRequiredService<NumPadViewModel>();
            _viewModel.PanelId = panelId;
            Loaded += (_, _) =>
            {
                DataContext = _viewModel;

                var window = Window.GetWindow(this);
                if (window == null)
                    throw new ApplicationException("Unable to instantiate NumPad window");

                _viewModel.PanelConfig.PanelHandle = new WindowInteropHelper(window).Handle;

                if (initialWidth == 0 && initialHeight == 0)
                {
                    this.Width = 300;
                    this.Height = 400;
                    _viewModel.PanelConfig.Width = Convert.ToInt16(this.Width);
                    _viewModel.PanelConfig.Height = Convert.ToInt32(this.Height);
                }
                else
                {
                    this.Width = initialWidth;
                    this.Height = initialHeight;
                }
            };

            this.MouseLeftButtonDown += NumPad_MouseLeftButtonDown;
            this.Topmost = true;
        }

        private void NumPad_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is Button btn) 
                _viewModel.ButtonCommand.Execute(btn.Name.Substring(3));
        }
        
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
