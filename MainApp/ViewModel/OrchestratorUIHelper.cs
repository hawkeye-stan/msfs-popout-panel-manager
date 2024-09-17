using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MSFSPopoutPanelManager.MainApp.AppWindow;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class OrchestratorUiHelper : BaseViewModel
    {
        private bool _minimizeForPopOut;
        
        public OrchestratorUiHelper(SharedStorage sharedStorage, PanelSourceOrchestrator panelSourceOrchestrator, PanelPopOutOrchestrator panelPopOutOrchestrator) : base(sharedStorage)
        {
            panelSourceOrchestrator.OnOverlayShowed -= HandleShowOverlay;
            panelSourceOrchestrator.OnOverlayShowed += HandleShowOverlay;

            panelSourceOrchestrator.OnNonEditOverlayShowed -= HandleShowNonEditOverlay;
            panelSourceOrchestrator.OnNonEditOverlayShowed += HandleShowNonEditOverlay;

            panelSourceOrchestrator.OnOverlayRemoved -= HandleRemoveOverlay;
            panelSourceOrchestrator.OnOverlayRemoved += HandleRemoveOverlay;

            panelPopOutOrchestrator.OnPopOutStarted -= HandleOnPopOutStarted;
            panelPopOutOrchestrator.OnPopOutStarted += HandleOnPopOutStarted;

            panelPopOutOrchestrator.OnPopOutCompleted -= HandleOnPopOutCompleted;
            panelPopOutOrchestrator.OnPopOutCompleted += HandleOnPopOutCompleted;

            panelPopOutOrchestrator.OnHudBarOpened -= HandleOnHudBarOpened;
            panelPopOutOrchestrator.OnHudBarOpened += HandleOnHudBarOpened;

            panelPopOutOrchestrator.OnNumPadOpened -= HandleOnNumPadOpened;
            panelPopOutOrchestrator.OnNumPadOpened += HandleOnNumPadOpened;

            panelPopOutOrchestrator.OnSwitchWindowOpened -= HandleOnSwitchWindowOpened;
            panelPopOutOrchestrator.OnSwitchWindowOpened += HandleOnSwitchWindowOpened;
        }

        private void HandleShowOverlay(object sender, PanelConfig panelConfig)
        {
            ShowOverlay(panelConfig);
        }

        private void HandleShowNonEditOverlay(object sender, PanelConfig panelConfig)
        {
            ShowOverlay(panelConfig, true);
        }

        private void HandleRemoveOverlay(object sender, PanelConfig panelConfig)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                for (int i = Application.Current.Windows.Count - 1; i >= 1; i--)
                {
                    if (Application.Current.Windows[i] is PanelCoorOverlay)
                    {
                        var panel = Application.Current.Windows[i] as PanelCoorOverlay;

                        if (panel?.PanelId == panelConfig.Id)
                        {
                            panel.Close();
                        }
                    }
                }
            });
        }

        private void HandleOnHudBarOpened(object sender, PanelConfig panelConfig)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                var hudBar = new HudBar(panelConfig.Id);
                hudBar.Show();

                await Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    WindowActionManager.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);
                    WindowActionManager.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);
                });
            });
        }

        private void HandleOnNumPadOpened(object sender, PanelConfig panelConfig)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                var numPad = new NumPad(panelConfig.Id, panelConfig.Width, panelConfig.Height);
                numPad.Show();

                await Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    WindowActionManager.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);
                    WindowActionManager.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);
                });
            });
        }

        private void HandleOnSwitchWindowOpened(object sender, PanelConfig panelConfig)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                var switchWindow = new SwitchWindow(panelConfig.Id, panelConfig.Width, panelConfig.Height);
                switchWindow.Show();

                await Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    WindowActionManager.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);
                    WindowActionManager.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);
                });
            });
        }

        private void HandleOnPopOutStarted(object sender, EventArgs e)
        {
            if (!AppSettingData.ApplicationSetting.PopOutSetting.MinimizeDuringPopOut)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Temporary minimize the app for pop out process
                _minimizeForPopOut = ApplicationWindow.WindowState != WindowState.Minimized;
                if (_minimizeForPopOut)
                    WindowActionManager.MinimizeWindow(ApplicationHandle);
            });
        }

        private void HandleOnPopOutCompleted(object sender, EventArgs arg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (AppSettingData.ApplicationSetting.PopOutSetting.MinimizeAfterPopOut)
                {
                    WindowActionManager.MinimizeWindow(ApplicationHandle);
                }
                else if (_minimizeForPopOut)
                {
                    ApplicationWindow.Show();
                    WindowActionManager.BringWindowToForeground(ApplicationHandle);
                }
                else if (!AppSettingData.ApplicationSetting.GeneralSetting.AlwaysOnTop)
                {
                    WindowActionManager.BringWindowToForeground(ApplicationHandle);
                }
            });

            _minimizeForPopOut = false;
        }

        private void ShowOverlay(PanelConfig panelConfig, bool nonEdit = false)
        {
            if (panelConfig.PanelType != PanelType.CustomPopout)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Remove existing overlay if exist
                HandleRemoveOverlay(this, panelConfig);

                var overlay = new PanelCoorOverlay(panelConfig.Id, !nonEdit)
                {
                    IsEditingPanelLocation = true,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };

                overlay.SetWindowCoor(Convert.ToInt32(panelConfig.PanelSource.X), Convert.ToInt32(panelConfig.PanelSource.Y));
                overlay.ShowInTaskbar = false;

                // Fix MS.Win32.UnsafeNativeMethods.GetWindowText exception
                try { overlay.Show(); } catch { overlay.Show(); }

                overlay.OnWindowLocationChanged += (_, e) =>
                {
                    var panelSource = ActiveProfile?.PanelConfigs.FirstOrDefault(p => p.Id == panelConfig.Id);
                    if (panelSource == null) 
                        return;

                    panelSource.PanelSource.X = e.X;
                    panelSource.PanelSource.Y = e.Y;
                };
            });
        }
    }
}
