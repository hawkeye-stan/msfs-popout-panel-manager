using System;
using System.Windows;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public abstract class BaseViewModel : ObservableObject
    {
        private SharedStorage SharedStorage { get; }

        protected const string ROOT_DIALOG_HOST = "RootDialog";

        protected BaseViewModel(SharedStorage sharedStorage)
        {
            SharedStorage = sharedStorage;
            InitializeChildPropertyChangeBinding();
        }

        public AppSettingData AppSettingData => SharedStorage.AppSettingData;

        public ProfileData ProfileData
        {
            get => SharedStorage.ProfileData;
            set => SharedStorage.ProfileData = value;
        }

        public FlightSimData FlightSimData => SharedStorage.FlightSimData;

        public UserProfile ActiveProfile => SharedStorage.ProfileData.ActiveProfile;

        public IntPtr ApplicationHandle
        {
            get => SharedStorage.ApplicationHandle;
            set => SharedStorage.ApplicationHandle = value;
        }

        public Window ApplicationWindow
        {
            get => SharedStorage.ApplicationWindow;
            set => SharedStorage.ApplicationWindow = value;
        }
    }
}
