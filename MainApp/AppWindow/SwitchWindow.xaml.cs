using Microsoft.Extensions.DependencyInjection;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace MSFSPopoutPanelManager.MainApp.AppWindow
{
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
                    this.Height = 60;
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

            if (_viewModel.ProfileData.ActiveProfile.ProfileSetting.SwitchWindowConfig.Panels != null)
            {
                var style = Application.Current.TryFindResource("MaterialDesignOutlinedButton") as Style;

                WrapPanelCustomButtons.Children.Clear();

                foreach (var panel in _viewModel.ProfileData.ActiveProfile.ProfileSetting.SwitchWindowConfig.Panels)
                {
                    WrapPanelCustomButtons.Children.Add(new Button()
                    {
                        Height = 45,
                        Margin = new Thickness(5, 5, 5, 5),
                        Content = panel.DisplayName,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 28,
                        Style = style,
                        Command = _viewModel.ButtonCommand,
                        CommandParameter = panel.PanelCaption
                    });
                }
            }
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
