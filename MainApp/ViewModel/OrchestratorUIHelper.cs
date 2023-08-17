using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class OrchestratorUIHelper : BaseViewModel
    {
        private bool _minimizeForPopOut;

        public OrchestratorUIHelper(MainOrchestrator orchestrator) : base(orchestrator)
        {
            Orchestrator.PanelSource.OnOverlayShowed += HandleShowOverlay;
            Orchestrator.PanelSource.OnNonEditOverlayShowed += HandleShowNonEditOverlay;
            Orchestrator.PanelSource.OnOverlayRemoved += HandleRemoveOverlay;

            Orchestrator.PanelPopOut.OnPopOutStarted += HandleOnPopOutStarted;
            Orchestrator.PanelPopOut.OnPopOutCompleted += HandleOnPopOutCompleted;
            Orchestrator.PanelPopOut.OnHudBarOpened += HandleOnHudBarOpened;
        }

        private void HandleShowOverlay(object? sender, PanelConfig panelConfig)
        {
            ShowOverlay(panelConfig, false);
        }

        private void HandleShowNonEditOverlay(object? sender, PanelConfig panelConfig)
        {
            ShowOverlay(panelConfig, true);
        }

        private void HandleRemoveOverlay(object? sender, PanelConfig panelConfig)
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
                            break;
                        }
                    }
                }
            });
        }

        private void HandleOnHudBarOpened(object? sender, PanelConfig panelConfig)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                HudBar hudBar = new HudBar(panelConfig.Id);
                hudBar.Show();

                Task.Run(async () =>
                {
                    Thread.Sleep(1000);
                    WindowActionManager.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);
                    WindowActionManager.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);
                });
            });
        }

        private void HandleOnPopOutStarted(object? sender, EventArgs e)
        {
            if (!Orchestrator.AppSettingData.ApplicationSetting.PopOutSetting.MinimizeDuringPopOut)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Temporary minimize the app for pop out process
                _minimizeForPopOut = Orchestrator.ApplicationWindow.WindowState != WindowState.Minimized;
                if (_minimizeForPopOut)
                    WindowActionManager.MinimizeWindow(Orchestrator.ApplicationHandle);
            });
        }

        private void HandleOnPopOutCompleted(object? sender, EventArgs arg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Orchestrator.AppSettingData.ApplicationSetting.PopOutSetting.MinimizeAfterPopOut)
                {
                    WindowActionManager.MinimizeWindow(Orchestrator.ApplicationHandle);
                }
                else if (_minimizeForPopOut)
                {
                    Orchestrator.ApplicationWindow.Show();
                    WindowActionManager.BringWindowToForeground(Orchestrator.ApplicationHandle);
                }
                else if (!Orchestrator.AppSettingData.ApplicationSetting.GeneralSetting.AlwaysOnTop)
                {
                    WindowActionManager.BringWindowToForeground(Orchestrator.ApplicationHandle);
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
                PanelCoorOverlay overlay = new PanelCoorOverlay(panelConfig.Id, !nonEdit);
                overlay.IsEditingPanelLocation = true;
                overlay.WindowStartupLocation = WindowStartupLocation.Manual;
                overlay.SetWindowCoor(Convert.ToInt32(panelConfig.PanelSource.X), Convert.ToInt32(panelConfig.PanelSource.Y));
                overlay.ShowInTaskbar = false;

                // Fix MS.Win32.UnsafeNativeMethods.GetWindowText exception
                try { overlay.Show(); } catch { overlay.Show(); }

                overlay.WindowLocationChanged += (sender, e) =>
                {
                    if (Orchestrator.ProfileData.ActiveProfile != null)
                    {
                        var panelSource = Orchestrator.ProfileData.ActiveProfile.PanelConfigs.FirstOrDefault(p => p.Id == panelConfig.Id);
                        if (panelSource != null)
                        {
                            panelSource.PanelSource.X = e.X;
                            panelSource.PanelSource.Y = e.Y;
                        }
                    }
                };
            });
        }
    }
}
