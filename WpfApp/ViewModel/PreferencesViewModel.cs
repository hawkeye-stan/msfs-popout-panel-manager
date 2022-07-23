using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.Shared;
using Prism.Commands;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    public class PreferencesViewModel : ObservableObject
    {
        private MainOrchestrator _orchestrator;

        public PreferencesViewModel(MainOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;

            ApplicationSettingsVisible = true;
            PopOutSettingsVisible = false;
            AutoPopOutSettingsVisible = false;
            TrackIRSettingsVisible = false;

            SectionSelectCommand = new DelegateCommand<object>(OnSectionSelected);
        }

        public DelegateCommand<object> SectionSelectCommand { get; private set; }

        public AppSettingData AppSettingData { get { return _orchestrator.AppSettingData; } }

        public bool ApplicationSettingsVisible { get; private set; }

        public bool PopOutSettingsVisible { get; private set; }

        public bool AutoPopOutSettingsVisible { get; private set; }

        public bool TrackIRSettingsVisible { get; private set; }

        public bool TouchSettingsVisible { get; private set; }

        public bool MSFSTouchPanelSettingsVisible { get; private set; }

        private void OnSectionSelected(object commandParameter)
        {
            ApplicationSettingsVisible = false;
            PopOutSettingsVisible = false;
            AutoPopOutSettingsVisible = false;
            TrackIRSettingsVisible = false;
            TouchSettingsVisible = false;
            MSFSTouchPanelSettingsVisible = false;

            switch (commandParameter.ToString())
            {
                case "Application Settings":
                    ApplicationSettingsVisible = true;
                    break;
                case "Pop Out Settings":
                    PopOutSettingsVisible = true;
                    break;
                case "Auto Pop Out Panel Settings":
                    AutoPopOutSettingsVisible = true;
                    break;
                case "Track IR Settings":
                    TrackIRSettingsVisible = true;
                    break;
                case "Touch Settings":
                    TouchSettingsVisible = true;
                    break;
                case "MSFS Touch Panel Settings":
                    MSFSTouchPanelSettingsVisible = true;
                    break;
            }
        }
    }
}
