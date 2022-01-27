using MSFSPopoutPanelManager.Model;
using MSFSPopoutPanelManager.Provider;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    public class PanelConfigurationViewModel
    {
        private UserProfileManager _userProfileManager;
        private PanelConfigurationManager _panelConfigurationManager;

        // Using PropertyChanged.Fody
        public event PropertyChangedEventHandler PropertyChanged;

        public DataStore DataStore { get; set; }

        public PanelConfigItem SelectedPanelConfigItem { get; set; }

        public DelegateCommand LockPanelsCommand => new DelegateCommand(OnLockPanelsChanged, CanExecute);
        public DelegateCommand PanelConfigUpdatedCommand => new DelegateCommand(OnPanelConfigUpdated, CanExecute);
        public DelegateCommand MinusTenPixelCommand => new DelegateCommand(OnDataItemIncDec, CanExecute);
        public DelegateCommand MinusOnePixelCommand => new DelegateCommand(OnDataItemIncDec, CanExecute);
        public DelegateCommand PlusOnePixelCommand => new DelegateCommand(OnDataItemIncDec, CanExecute);
        public DelegateCommand PlusTenPixelCommand => new DelegateCommand(OnDataItemIncDec, CanExecute);

        public PanelConfigurationViewModel(DataStore dataStore, UserProfileManager userProfileManager)
        {
            DataStore = dataStore;
            _userProfileManager = userProfileManager;
            _panelConfigurationManager = new PanelConfigurationManager(_userProfileManager);
        }

        public void Initialize()
        {
            _userProfileManager.UserProfiles = DataStore.UserProfiles;
            _panelConfigurationManager.UserProfile = DataStore.ActiveUserProfile;
            _panelConfigurationManager.AllowEdit = DataStore.AllowEdit;
            _panelConfigurationManager.HookWinEvent();
            DataStore.OnAllowEditChanged += (sender, e) => 
            {
                _panelConfigurationManager.AllowEdit = DataStore.AllowEdit; 
            };
        }

        public void UnhookWinEvents()
        {
            _panelConfigurationManager.UnhookWinEvent();
        }

        private void OnLockPanelsChanged(object commandParamenter)
        {
            _panelConfigurationManager.LockPanelsUpdated();
        }

        private void OnPanelConfigUpdated(object panelConfigItem)
        {
            if(DataStore.AllowEdit)
                _panelConfigurationManager.PanelConfigPropertyUpdated(panelConfigItem as PanelConfigItem);
        }

        private void OnDataItemIncDec(object commandParameter)
        {
            if (DataStore.AllowEdit)
                _panelConfigurationManager.PanelConfigIncreaseDecrease(SelectedPanelConfigItem, Convert.ToInt32(commandParameter));
        }

        private bool CanExecute(object commandParameter)
        {
            return true;
        }
    }  
}
