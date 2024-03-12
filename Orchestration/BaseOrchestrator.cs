using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;

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

        protected void CloseAllPopOuts()
        {
            foreach (var panelConfig in ProfileData.ActiveProfile.PanelConfigs)
            {
                if (panelConfig.FloatingPanel.IsEnabled && !panelConfig.IsFloating)
                    panelConfig.IsFloating = true;
            }

            WindowActionManager.CloseAllPopOuts();
        }
    }
}
