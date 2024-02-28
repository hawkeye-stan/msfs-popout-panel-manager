using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.Orchestration
{
    public abstract class BaseOrchestrator : ObservableObject
    {
        private readonly SharedStorage _sharedStorage;

        protected BaseOrchestrator(SharedStorage sharedStorage)
        {
            _sharedStorage = sharedStorage;
        }

        protected ProfileData ProfileData => _sharedStorage.ProfileData;

        protected AppSettingData AppSettingData => _sharedStorage.AppSettingData;

        protected FlightSimData FlightSimData => _sharedStorage.FlightSimData;
    }
}
