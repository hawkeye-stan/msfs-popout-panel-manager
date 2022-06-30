using CommunityToolkit.Mvvm.ComponentModel;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    public class PreferencesViewModel : ObservableObject
    {
        public PreferencesViewModel(DataStore dataStore)
        {
            DataStore = dataStore;

            ApplicationSettingsVisible = true;
            PopOutSettingsVisible = false;
            AutoPopOutSettingsVisible = false;
            TrackIRSettingsVisible = false;
        }

        public DelegateCommand SectionSelectCommand => new DelegateCommand(OnSectionSelected, CanExecute);

        public DataStore DataStore { get; private set; }

        public bool ApplicationSettingsVisible { get; private set; }

        public bool PopOutSettingsVisible { get; private set; }

        public bool AutoPopOutSettingsVisible { get; private set; }

        public bool TrackIRSettingsVisible { get; private set; }

        private bool CanExecute(object commandParameter)
        {
            return true;
        }

        private void OnSectionSelected(object commandParameter)
        {
            ApplicationSettingsVisible = false;
            PopOutSettingsVisible = false;
            AutoPopOutSettingsVisible = false;
            TrackIRSettingsVisible = false;

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
            }
        }
    }
}
