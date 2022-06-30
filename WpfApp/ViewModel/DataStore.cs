using CommunityToolkit.Mvvm.ComponentModel;
using MSFSPopoutPanelManager.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    public class DataStore : ObservableObject
    {
        private int _activeProfileId;
        private AppSetting _appSetting;

        public event EventHandler OnActiveUserProfileChanged;
        public event EventHandler OnAllowEditChanged;

        public DataStore()
        {
            _activeProfileId = -1;
            _allowEdit = true;
            IsFlightActive = true;      // ToDo: temporary for testing
        }

        public AppSetting AppSetting
        {
            get
            {
                return _appSetting;
            }
            set
            {
                _appSetting = value;

                if (_appSetting != null)
                {

                    IsEnableAutoPopOutPanel = _appSetting.AutoPopOutPanels;
                }

                // bubble event up to this 'DataStore' level


                _appSetting.AutoPopOutPanelsChanged += (sender, e) =>
                {
                    IsEnableAutoPopOutPanel = e.Value;
                };
            }
        }

        public ObservableCollection<UserProfile> UserProfiles { get; set; }

        public ObservableCollection<PanelSourceCoordinate> ActiveProfilePanelCoordinates
        {
            get
            {
                if (ActiveUserProfile == null)
                    return new ObservableCollection<PanelSourceCoordinate>();
                else
                    return ActiveUserProfile.PanelSourceCoordinates;
            }
        }

        public ObservableCollection<PanelConfig> ActiveProfilePanelConfigs
        {
            get
            {
                if (ActiveUserProfile == null)
                    return new ObservableCollection<PanelConfig>();
                else
                    return ActiveUserProfile.PanelConfigs;
            }
        }

        public UserProfile ActiveUserProfile
        {
            get
            {
                if (ActiveUserProfileId == -1)
                    return null;
                else
                    return UserProfiles.ToList().Find(x => x.ProfileId == ActiveUserProfileId);
            }
        }

        private bool _allowEdit;
        public bool AllowEdit
        {
            get
            {
                return _allowEdit;
            }
            set
            {
                if (value != _allowEdit)
                {
                    _allowEdit = value;
                    OnAllowEditChanged?.Invoke(this, null);
                }
            }
        }

        public IntPtr ApplicationHandle { get; set; }

        public Window ApplicationWindow { get; set; }

        public int ActiveUserProfileId
        {
            get
            {
                return _activeProfileId;
            }
            set
            {
                _activeProfileId = value;

                // Set active profile flag
                UserProfiles.ToList().ForEach(p => p.IsActive = false);
                var profile = UserProfiles.ToList().Find(p => p.ProfileId == value);
                if (profile != null)
                    profile.IsActive = true;

                OnActiveUserProfileChanged?.Invoke(this, null);
            }
        }

        public bool HasActiveUserProfileId
        {
            get { return ActiveUserProfileId != -1; }
        }

        public string CurrentMsfsPlaneTitle { get; set; }

        public bool HasCurrentMsfsPlaneTitle
        {
            get { return !String.IsNullOrEmpty(CurrentMsfsPlaneTitle); }
        }

        public bool IsAircraftBoundToProfile
        {
            get
            {
                if (ActiveUserProfile == null)
                    return false;

                return ActiveUserProfile.BindingAircraftLiveries.ToList().Exists(p => p == CurrentMsfsPlaneTitle);
            }
        }

        public bool IsAllowedDeleteAircraftBinding
        {
            get
            {
                if (ActiveUserProfile == null || !HasCurrentMsfsPlaneTitle)
                    return false;

                var uProfile = UserProfiles.ToList().Find(u => u.BindingAircraftLiveries.ToList().Exists(p => p == CurrentMsfsPlaneTitle));
                if (uProfile != null && uProfile.ProfileId != ActiveUserProfileId)
                    return false;

                return ActiveUserProfile.BindingAircraftLiveries.ToList().Exists(p => p == CurrentMsfsPlaneTitle);
            }
        }

        public bool IsAllowedAddAircraftBinding
        {
            get
            {
                if (ActiveUserProfile == null || !HasCurrentMsfsPlaneTitle)
                    return false;

                var uProfile = UserProfiles.ToList().Find(u => u.BindingAircraftLiveries.ToList().Exists(p => p == CurrentMsfsPlaneTitle));
                if (uProfile != null && uProfile.ProfileId != ActiveUserProfileId)
                    return false;

                return !ActiveUserProfile.BindingAircraftLiveries.ToList().Exists(p => p == CurrentMsfsPlaneTitle);
            }
        }

        public bool ElectricalMasterBatteryStatus { get; set; }

        public bool IsSimulatorStarted { get; set; }

        public bool IsEnteredFlight { get; set; }

        public bool IsFlightActive { get; set; }
        public bool IsEnableAutoPopOutPanel { get; set; }
    }
}