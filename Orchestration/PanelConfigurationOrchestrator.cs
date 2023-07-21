using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class PanelConfigurationOrchestrator : ObservableObject
    {
        private ProfileData _profileData;
        private AppSettingData _appSettingData;
        private FlightSimData _flightSimData;

        public PanelConfigurationOrchestrator(ProfileData profileData, AppSettingData appSettingData, FlightSimData flightSimData)
        {
            _profileData = profileData;
            _appSettingData = appSettingData;
            _flightSimData = flightSimData;

            _appSettingData.EnablePanelResetWhenLockedChanged += (sender, e) =>
            {
                if (flightSimData.IsInCockpit)
                    StartConfiguration();
            };
            _profileData.ActiveProfileChanged += (sender, e) => { EndConfiguration(); EndTouchHook(); };
        }

        private UserProfile ActiveProfile { get { return _profileData == null ? null : _profileData.ActiveProfile; } }

        public void StartConfiguration()
        {
            if (!ActiveProfile.IsPoppedOut)
                return;

            Debug.WriteLine("Starting Panel Configuration...");

            WindowEventManager.ActiveProfile = _profileData.ActiveProfile;
            WindowEventManager.ApplicationSetting = _appSettingData.ApplicationSetting;
            TouchEventManager.ActiveProfile = _profileData.ActiveProfile;
            TouchEventManager.ApplicationSetting = _appSettingData.ApplicationSetting;
            GameRefocusManager.ApplicationSetting = _appSettingData.ApplicationSetting;

            // Must use application dispatcher to dispatch UI events (winEventHook)
            Application.Current.Dispatcher.Invoke(() =>
            {
                WindowEventManager.HookWinEvent();
            });
        }

        public void EndConfiguration()
        {
            Debug.WriteLine("Ending Panel Configuration...");

            Application.Current.Dispatcher.Invoke(() =>
            {
                WindowEventManager.UnhookWinEvent();
            });
        }

        public void StartTouchHook()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TouchEventManager.UnHook();

                if (!ActiveProfile.IsPoppedOut)
                    return;

                var hasTouchEnabledPanel = ActiveProfile.PanelConfigs.Any(p => p.TouchEnabled && p.IsPopOutSuccess != null && (bool)p.IsPopOutSuccess);

                if (hasTouchEnabledPanel && !TouchEventManager.IsHooked)
                    TouchEventManager.Hook();
            });
        }

        public void EndTouchHook()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TouchEventManager.UnHook();
            });
        }

        public void PanelConfigPropertyUpdated(IntPtr panelHandle, PanelConfigPropertyName configPropertyName)
        {
            if (panelHandle == IntPtr.Zero || ActiveProfile.IsLocked || !ActiveProfile.IsPoppedOut)
                return;

            var panelConfig = ActiveProfile.PanelConfigs.FirstOrDefault(p => p.PanelHandle == panelHandle);

            if (panelConfig != null)
            {
                if (configPropertyName == PanelConfigPropertyName.FullScreen)
                {
                    InputEmulationManager.ToggleFullScreenPanel(panelConfig.PanelHandle);

                    if (!panelConfig.FullScreen)
                        WindowActionManager.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);
                }
                else if (configPropertyName == PanelConfigPropertyName.PanelName)
                {
                    var name = panelConfig.PanelName;

                    if (panelConfig.PanelType == PanelType.CustomPopout && name.IndexOf("(Custom)") == -1)
                    {
                        name = name + " (Custom)";
                        WindowActionManager.SetWindowCaption(panelConfig.PanelHandle, name);
                    }
                }
                else if (!panelConfig.FullScreen)
                {
                    switch (configPropertyName)
                    {
                        case PanelConfigPropertyName.Left:
                        case PanelConfigPropertyName.Top:
                            WindowActionManager.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);
                            break;
                        case PanelConfigPropertyName.Width:
                        case PanelConfigPropertyName.Height:
                            if (panelConfig.PanelType == PanelType.HudBarWindow)
                                return;

                            if (panelConfig.HideTitlebar)
                            {
                                WindowActionManager.ApplyHidePanelTitleBar(panelConfig.PanelHandle, false);
                                Thread.Sleep(100);
                            }

                            WindowActionManager.MoveWindow(panelConfig.PanelHandle, panelConfig.Left, panelConfig.Top, panelConfig.Width, panelConfig.Height);

                            if (panelConfig.HideTitlebar)
                            {
                                Thread.Sleep(100);
                                WindowActionManager.ApplyHidePanelTitleBar(panelConfig.PanelHandle, true);
                                Thread.Sleep(100);
                            }

                            break;
                        case PanelConfigPropertyName.AlwaysOnTop:
                            WindowActionManager.ApplyAlwaysOnTop(panelConfig.PanelHandle, panelConfig.PanelType, panelConfig.AlwaysOnTop);
                            break;
                        case PanelConfigPropertyName.HideTitlebar:
                            WindowActionManager.ApplyHidePanelTitleBar(panelConfig.PanelHandle, panelConfig.HideTitlebar);
                            break;
                        case PanelConfigPropertyName.TouchEnabled:
                            if (ActiveProfile.IsPoppedOut)
                                WindowEventManager.HookWinEvent();

                            if (ActiveProfile.PanelConfigs.Any(p => p.TouchEnabled) && !TouchEventManager.IsHooked)         // only start hook if it has not been started
                                StartTouchHook();
                            else if (ActiveProfile.PanelConfigs.All(p => !p.TouchEnabled) && TouchEventManager.IsHooked)    // only disable hook if no more panel is using touch
                                EndTouchHook();

                            break;
                        case PanelConfigPropertyName.AutoGameRefocus:
                            if (ActiveProfile.IsPoppedOut)
                                WindowEventManager.HookWinEvent();
                            break;
                    }
                }

                _profileData.WriteProfiles();
            }
        }
    }
}
