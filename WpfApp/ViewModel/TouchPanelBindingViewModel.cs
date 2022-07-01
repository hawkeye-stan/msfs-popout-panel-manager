using CommunityToolkit.Mvvm.ComponentModel;
using MSFSPopoutPanelManager.Model;
using MSFSPopoutPanelManager.Provider;
using MSFSPopoutPanelManager.TouchPanel.Shared;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    public class TouchPanelBindingViewModel : ObservableObject
    {
        private DataStore _dataStore;
        private UserProfileManager _userProfileManager;

        public event EventHandler OnBindingUpdated;

        public DelegateCommand PanelSelectCommand => new DelegateCommand(OnPanelSelected, CanExecute);

        public ObservableCollection<PlaneProfile> Profiles { get; set; }

        public TouchPanelBindingViewModel(DataStore dataStore, UserProfileManager userProfileManager)
        {
            _dataStore = dataStore;
            _userProfileManager = userProfileManager;
        }

        public void Initialize()
        {
            if (_dataStore.ActiveUserProfile == null)
                return;

            var dataItems = new ObservableCollection<PlaneProfileInfo>(ConfigurationReader.GetPlaneProfilesConfiguration());

            // Map plane profiles to bindable objects
            Profiles = new ObservableCollection<PlaneProfile>();

            foreach (var dataItem in dataItems)
            {
                var planeProfile = new PlaneProfile();
                planeProfile.PlaneId = dataItem.PlaneId;
                planeProfile.Name = dataItem.Name;

                if (dataItem.Panels != null && dataItem.Panels.Count > 0)
                {
                    planeProfile.Panels = new ObservableCollection<PanelProfile>();

                    foreach (var dataItemPanel in dataItem.Panels)
                    {
                        planeProfile.Panels.Add(new PanelProfile()
                        {
                            PlaneId = dataItem.PlaneId,
                            PanelId = dataItemPanel.PanelId,
                            Name = dataItemPanel.Name,
                            IsSelected = _dataStore.ActiveUserProfile.TouchPanelBindings == null ? false : _dataStore.ActiveUserProfile.TouchPanelBindings.ToList().Exists(b => b.PlaneId == dataItem.PlaneId && b.PanelId == dataItemPanel.PanelId)
                        });
                    }
                }

                Profiles.Add(planeProfile);
            }
        }

        private bool CanExecute(object commandParameter)
        {
            return true;
        }

        private void OnPanelSelected(object commandParameter)
        {
            _dataStore.ActiveUserProfile.PanelConfigs.ToList().RemoveAll(panelConfig => panelConfig.PanelType == PanelType.MSFSTouchPanel);
            _dataStore.ActiveUserProfile.TouchPanelBindings.Clear();
            _dataStore.ActiveUserProfile.IsLocked = false;

            foreach (var planeProfile in Profiles)
            {
                foreach(var panel in planeProfile.Panels)
                {
                    if(panel.IsSelected)
                        _dataStore.ActiveUserProfile.TouchPanelBindings.Add(new TouchPanelBinding() { PlaneId = planeProfile.PlaneId, PanelId = panel.PanelId });
                }
            }

            _userProfileManager.WriteUserProfiles();

            OnBindingUpdated?.Invoke(this, null);
        }
    }

    public class PlaneProfile
    {
        public string PlaneId { get; set; }

        public string Name { get; set; }

        public ObservableCollection<PanelProfile> Panels { get; set; }

        public bool HasSelected
        {
            get
            {
                return Panels.ToList().Exists(p => p.IsSelected);
            }
        }
    }

    public class PanelProfile
    {
        public string PlaneId { get; set; }

        public string PanelId { get; set; }

        public string Name { get; set; }

        public bool IsSelected { get; set; }
    }
}
