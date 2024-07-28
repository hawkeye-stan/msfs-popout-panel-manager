using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class ProfileData : ObservableObject
    {
        public event PropertyChangedEventHandler OnActiveProfileChanged;
        public event EventHandler<bool> OnUseFloatingPanelChanged;

        public SortedObservableCollection<UserProfile> Profiles { get; private set; } = new();

        [IgnorePropertyChanged]
        internal FlightSimData FlightSimDataRef { private get; set; }

        [IgnorePropertyChanged]
        public AppSettingData AppSettingDataRef { private get; set; }

        public void AddProfile(string profileName)
        {
            var newProfile = new UserProfile { Name = profileName, IsUsedLegacyCameraSystem = false };
            newProfile.OnProfileChanged += (_, _) => WriteProfiles();

            Profiles.Add(newProfile);
            SetActiveProfile(newProfile.Id);

            ProfileDataManager.WriteProfiles(Profiles, AppSettingDataRef.ApplicationSetting.GeneralSetting.UseApplicationDataPath);

            AppSettingDataRef.ApplicationSetting.SystemSetting.LastUsedProfileId = newProfile.Id;
        }

        public void AddProfile(string profileName, UserProfile copiedProfile)
        {
            if (copiedProfile == null)
                return;

            var newProfile = new UserProfile { Name = profileName};

            foreach (var copiedPanelConfig in copiedProfile.PanelConfigs)
            {
                var copied = copiedPanelConfig.Copy();

                copied.Id = Guid.NewGuid();
                copied.PanelHandle = IntPtr.MaxValue;
                copied.IsEditingPanel = false;
                copied.IsSelectedPanelSource = false;
                copied.IsShownPanelSource = false;

                newProfile.PanelConfigs.Add(copied);
            }

            newProfile.ProfileSetting = copiedProfile.ProfileSetting.Copy();
            newProfile.MsfsGameWindowConfig = copiedProfile.MsfsGameWindowConfig.Copy();
            newProfile.PanelSourceCockpitZoomFactor = copiedProfile.PanelSourceCockpitZoomFactor;
            newProfile.IsUsedLegacyCameraSystem = copiedProfile.IsUsedLegacyCameraSystem;

            newProfile.OnProfileChanged += (_, _) => WriteProfiles();

            Profiles.Add(newProfile);
            SetActiveProfile(newProfile.Id);

            ProfileDataManager.WriteProfiles(Profiles, AppSettingDataRef.ApplicationSetting.GeneralSetting.UseApplicationDataPath);

            AppSettingDataRef.ApplicationSetting.SystemSetting.LastUsedProfileId = newProfile.Id;
        }

        public bool DeleteActiveProfile()
        {
            if (ActiveProfile == null)
                return false;

            var activeProfileIndex = Profiles.IndexOf(ActiveProfile);

            Profiles.Remove(ActiveProfile);

            if (activeProfileIndex == 0 && Profiles.Count == 0)
                SetActiveProfile(-1);
            else if (activeProfileIndex == Profiles.Count)
                SetActiveProfile(0);
            else
                SetActiveProfile(activeProfileIndex);

            return true;
        }

        public void AddProfileBinding(string aircraft)
        {
            if (ActiveProfile == null)
                return;

            var boundProfile = Profiles.FirstOrDefault(p => p.AircraftBindings.Any(a => a == aircraft));
            if (boundProfile != null)
                return;

            ActiveProfile.AircraftBindings.Add(aircraft);

            ProfileDataManager.WriteProfiles(Profiles, AppSettingDataRef.ApplicationSetting.GeneralSetting.UseApplicationDataPath);
            RefreshProfile();
        }

        public void DeleteProfileBinding(string aircraft)
        {
            if (ActiveProfile == null)
                return;

            ActiveProfile.AircraftBindings.Remove(aircraft);

            ProfileDataManager.WriteProfiles(Profiles, AppSettingDataRef.ApplicationSetting.GeneralSetting.UseApplicationDataPath);
            RefreshProfile();
        }

        public void ReadProfiles()
        {
            Profiles = new SortedObservableCollection<UserProfile>(ProfileDataManager.ReadProfiles(AppSettingDataRef.ApplicationSetting.GeneralSetting.UseApplicationDataPath));
            Profiles.ToList().ForEach(p => p.OnProfileChanged += (_, _) => WriteProfiles());

            // Detect profiles collection changes
            Profiles.CollectionChanged += (_, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        WriteProfiles();
                        break;
                }
            };
        }

        public void WriteProfiles()
        {
            Debug.WriteLine("Saving Data ... ");
            ProfileDataManager.WriteProfiles(Profiles, AppSettingDataRef.ApplicationSetting.GeneralSetting.UseApplicationDataPath);
        }

        public void SetActiveProfile(Guid id)
        {
            StatusMessageWriter.ClearMessage();

            if (id == Guid.Empty && Profiles.Count == 0)
            {
                ActiveProfile = null;
                AppSettingDataRef.ApplicationSetting.SystemSetting.LastUsedProfileId = Guid.Empty;
            }
            else if (id == Guid.Empty && Profiles.Count > 0)
            {
                ActiveProfile = Profiles.First();
                AppSettingDataRef.ApplicationSetting.SystemSetting.LastUsedProfileId = ActiveProfile.Id;
            }
            else
            {
                ActiveProfile = Profiles.FirstOrDefault(p => p.Id == id);

                switch (ActiveProfile)
                {
                    case null when Profiles.Count > 0:
                        ActiveProfile = Profiles.First();
                        AppSettingDataRef.ApplicationSetting.SystemSetting.LastUsedProfileId = ActiveProfile.Id;
                        break;
                    case null when Profiles.Count == 0:
                        AppSettingDataRef.ApplicationSetting.SystemSetting.LastUsedProfileId = Guid.Empty;
                        return;
                    default:
                    {
                        if(ActiveProfile != null)
                            AppSettingDataRef.ApplicationSetting.SystemSetting.LastUsedProfileId = ActiveProfile.Id;
                        break;
                    }
                }
            }

            ResetActiveProfile();

            // Set active profile flag, this is used only for UI binding
            if (Profiles.Count <= 0) 
                return;
            
            Profiles.ToList().ForEach(p => p.IsActive = false);

            if (ActiveProfile != null)
            {
                ActiveProfile.IsActive = true;
                ActiveProfile.OnUseFloatingPanelChanged += (sender, e) => OnUseFloatingPanelChanged?.Invoke(sender, e);
            }

            OnActiveProfileChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }

        public void SetActiveProfile(int profileIndex)
        {
            SetActiveProfile(profileIndex == -1 ? Guid.Empty : Profiles[profileIndex].Id);
        }

        public UserProfile ActiveProfile { get; private set; }

        public bool IsAircraftBoundToProfile
        {
            get
            {
                if (ActiveProfile == null)
                    return false;

                return ActiveProfile.AircraftBindings.Any(p => p == FlightSimDataRef.AircraftName);
            }
        }

        public bool IsAllowedAddAircraftBinding
        {
            get
            {
                if (ActiveProfile == null || !FlightSimDataRef.HasAircraftName)
                    return true;

                var uProfile = Profiles.FirstOrDefault(u => u.AircraftBindings.Any(p => p == FlightSimDataRef.AircraftName));
                if (uProfile != null && uProfile != ActiveProfile)
                    return false;

                return true;
            }
        }

        public void RefreshProfile()
        {
            int profileIndex;
            if (ActiveProfile == null)
                profileIndex = -1;
            else
                profileIndex = Profiles.IndexOf(ActiveProfile);

            ActiveProfile = null;

            SetActiveProfile(profileIndex == -1 ? Guid.Empty : Profiles[profileIndex].Id);
        }

        public void ResetActiveProfile()
        {
            InputHookManager.EndMouseHook();
            //InputHookManager.EndKeyboardHook();

            if (ActiveProfile == null)
                return;

            ActiveProfile.CurrentMoveResizePanelId = Guid.Empty;
            ActiveProfile.IsEditingPanelSource = false;
            ActiveProfile.IsSelectingPanelSource = false;
            ActiveProfile.IsPoppedOut = false;
            
            foreach (var panelConfig in ActiveProfile.PanelConfigs)
            {
                panelConfig.IsSelectedPanelSource = false;
                panelConfig.IsEditingPanel = false;
                panelConfig.PanelHandle = IntPtr.MaxValue;
            }
        }

        public void AutoSwitchProfile()
        {
            // Automatic switching of active profile when SimConnect active aircraft changes
            if (Profiles != null && !string.IsNullOrEmpty(FlightSimDataRef.AircraftName))
            {
                var matchedProfile = Profiles.FirstOrDefault(p => p.AircraftBindings.Any(t => t == FlightSimDataRef.AircraftName));
                if (matchedProfile != null)
                {
                    SetActiveProfile(matchedProfile.Id);
                    RefreshProfile();
                }
            }
        }

        public void SaveMsfsGameWindowConfig()
        {
            if (ActiveProfile == null)
                return;

            var msfsGameWindowConfig = WindowActionManager.GetMsfsGameWindowLocation();
            if (msfsGameWindowConfig.IsValid)
            {
                ActiveProfile.MsfsGameWindowConfig = msfsGameWindowConfig;
                WriteProfiles();
            }
        }
    }
}
