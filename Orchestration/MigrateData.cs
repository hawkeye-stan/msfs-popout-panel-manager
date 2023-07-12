using MSFSPopoutPanelManager.DomainModel.Legacy;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.DomainModel.Setting;
using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MSFSPopoutPanelManager.Orchestration
{
    internal class MigrateData
    {
        private const string USER_PROFILE_DATA_FILENAME = "userprofiledata.json";
        private const string APP_SETTING_DATA_FILENAME = "appsettingdata.json";
        private const string ERROR_LOG_FILENAME = "error.log";
        private const string INFO_LOG_FILENAME = "info.log";
        private const string DEBUG_LOG_FILENAME = "debug.log";

        public static ApplicationSetting MigrateAppSettingFile(string content)
        {
            try
            {
                BackupAppSettingFile();

                var appSetting = new ApplicationSetting();
                var legacyAppSetting = JsonConvert.DeserializeObject<LegacyAppSetting>(content);

                // General settings
                appSetting.GeneralSetting.AlwaysOnTop = legacyAppSetting.AlwaysOnTop;
                appSetting.GeneralSetting.AutoClose = legacyAppSetting.AutoClose;
                appSetting.GeneralSetting.MinimizeToTray = legacyAppSetting.MinimizeToTray;
                appSetting.GeneralSetting.StartMinimized = legacyAppSetting.StartMinimized;

                // Auto pop out setting
                appSetting.AutoPopOutSetting.IsEnabled = legacyAppSetting.AutoPopOutPanels;

                // Pop out setting
                appSetting.PopOutSetting.UseLeftRightControlToPopOut = legacyAppSetting.UseLeftRightControlToPopOut;
                appSetting.PopOutSetting.MinimizeAfterPopOut = legacyAppSetting.MinimizeAfterPopOut;
                appSetting.PopOutSetting.AutoPanning.IsEnabled = legacyAppSetting.UseAutoPanning;
                appSetting.PopOutSetting.AutoPanning.KeyBinding = legacyAppSetting.AutoPanningKeyBinding;
                appSetting.PopOutSetting.AfterPopOutCameraView.IsEnabled = legacyAppSetting.AfterPopOutCameraView.EnableReturnToCameraView;
                appSetting.PopOutSetting.AfterPopOutCameraView.CameraView = legacyAppSetting.AfterPopOutCameraView.CameraView;
                appSetting.PopOutSetting.AfterPopOutCameraView.KeyBinding = legacyAppSetting.AfterPopOutCameraView.CustomCameraKeyBinding;

                // Refocus setting
                appSetting.RefocusSetting.RefocusGameWindow.IsEnabled = legacyAppSetting.TouchScreenSettings.RefocusGameWindow;

                var delay = Math.Round(legacyAppSetting.TouchScreenSettings.RefocusGameWindowDelay / 1000.0, 1);
                appSetting.RefocusSetting.RefocusGameWindow.Delay = delay < 1.0 ? 1.0 : delay;

                // Touch setting
                appSetting.TouchSetting.TouchDownUpDelay = 0;

                // Track IR setting
                appSetting.TrackIRSetting.AutoDisableTrackIR = legacyAppSetting.AutoDisableTrackIR;

                // Windowed mode setting
                appSetting.WindowedModeSetting.AutoResizeMsfsGameWindow = legacyAppSetting.AutoResizeMsfsGameWindow;

                return appSetting;
            }
            catch (Exception ex)
            {
                var msg = "An unknown application setting data migration error has occured. Application will close";
                FileLogger.WriteException(msg, ex);

                Environment.Exit(0);
            }

            return null;
        }

        public static IList<UserProfile> MigrateUserProfileFile(string content)
        {
            try
            {
                BackupUserProfileFile();

                var profiles = new List<UserProfile>();
                var legacyProfiles = JsonConvert.DeserializeObject<List<LegacyProfile>>(content);
                legacyProfiles = legacyProfiles.OrderBy(p => p.ProfileName.ToLower()).ToList();

                foreach (var legacyProfile in legacyProfiles)
                {
                    var profile = new UserProfile();

                    profile.Name = legacyProfile.ProfileName;
                    profile.IsLocked = legacyProfile.IsLocked;
                    profile.AircraftBindings = legacyProfile.BindingAircrafts;

                    profile.ProfileSetting.PowerOnRequiredForColdStart = legacyProfile.PowerOnRequiredForColdStart;
                    profile.ProfileSetting.IncludeInGamePanels = legacyProfile.IncludeInGamePanels;

                    if (legacyProfile.MsfsGameWindowConfig != null)
                    {
                        profile.MsfsGameWindowConfig.Top = legacyProfile.MsfsGameWindowConfig.Top;
                        profile.MsfsGameWindowConfig.Left = legacyProfile.MsfsGameWindowConfig.Left;
                        profile.MsfsGameWindowConfig.Width = legacyProfile.MsfsGameWindowConfig.Width;
                        profile.MsfsGameWindowConfig.Height = legacyProfile.MsfsGameWindowConfig.Height;
                    }

                    foreach (var legacyPanelConfig in legacyProfile.PanelConfigs)
                    {
                        var panelConfig = new PanelConfig();

                        panelConfig.PanelName = legacyPanelConfig.PanelName;
                        panelConfig.PanelType = legacyPanelConfig.PanelType;

                        panelConfig.Top = legacyPanelConfig.Top;
                        panelConfig.Left = legacyPanelConfig.Left + 9;
                        panelConfig.Width = legacyPanelConfig.Width - 18;
                        panelConfig.Height = legacyPanelConfig.Height - 9;

                        if (panelConfig.PanelType == PanelType.CustomPopout)
                        {
                            var legacyPanelSource = legacyProfile.PanelSourceCoordinates.FirstOrDefault(x => x.PanelIndex == legacyPanelConfig.PanelIndex);

                            if (legacyPanelSource != null)
                            {
                                panelConfig.PanelSource.X = legacyPanelSource.X;
                                panelConfig.PanelSource.Y = legacyPanelSource.Y;
                                panelConfig.PanelSource.Color = PanelConfigColors.GetNextAvailableColor(profile.PanelConfigs.ToList());
                            }
                        }

                        panelConfig.AlwaysOnTop = legacyPanelConfig.AlwaysOnTop;
                        panelConfig.HideTitlebar = legacyPanelConfig.HideTitlebar;
                        panelConfig.FullScreen = legacyPanelConfig.FullScreen;

                        if (legacyProfile.RealSimGearGTN750Gen1Override)
                            panelConfig.TouchEnabled = false;
                        else
                            panelConfig.TouchEnabled = legacyPanelConfig.TouchEnabled;

                        if (legacyPanelConfig.DisableGameRefocus || panelConfig.PanelType == PanelType.BuiltInPopout)
                            panelConfig.AutoGameRefocus = false;

                        profile.PanelConfigs.Add(panelConfig);
                    }

                    profiles.Add(profile);
                }

                return profiles;
            }
            catch (Exception ex)
            {
                var msg = "An unknown user data migration error has occured. Application will close";
                FileLogger.WriteException(msg, ex);

                Environment.Exit(0);
            }

            return null;
        }

        private static void BackupAppSettingFile()
        {
            var srcPath = Path.Combine(FileIo.GetUserDataFilePath(), APP_SETTING_DATA_FILENAME);
            var backupPath = Path.Combine(FileIo.GetUserDataFilePath(), "Backup-previous-version", APP_SETTING_DATA_FILENAME);

            if (File.Exists(srcPath))
            {
                Directory.CreateDirectory(Path.Combine(FileIo.GetUserDataFilePath(), "Backup-previous-version"));
                File.Copy(srcPath, backupPath, true);
            }

            // Delete existing error log
            var logFilePath = Path.Combine(FileIo.GetUserDataFilePath(), "LogFiles", ERROR_LOG_FILENAME);
            if (File.Exists(logFilePath))
                File.Delete(logFilePath);

            logFilePath = Path.Combine(FileIo.GetUserDataFilePath(), "LogFiles", INFO_LOG_FILENAME);
            if (File.Exists(logFilePath))
                File.Delete(logFilePath);

            logFilePath = Path.Combine(FileIo.GetUserDataFilePath(), "LogFiles", DEBUG_LOG_FILENAME);
            if (File.Exists(logFilePath))
                File.Delete(logFilePath);

            FileLogger.WriteLog("File initialized...", StatusMessageType.Error);
        }

        private static void BackupUserProfileFile()
        {
            var srcPath = Path.Combine(FileIo.GetUserDataFilePath(), USER_PROFILE_DATA_FILENAME);
            var backupPath = Path.Combine(FileIo.GetUserDataFilePath(), "Backup-previous-version", USER_PROFILE_DATA_FILENAME);

            if (File.Exists(srcPath))
            {
                Directory.CreateDirectory(Path.Combine(FileIo.GetUserDataFilePath(), "Backup-previous-version"));
                File.Copy(srcPath, backupPath, true);
            }
        }
    }
}
