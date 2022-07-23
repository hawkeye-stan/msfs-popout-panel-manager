using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.Shared;
using Prism.Commands;
using System.Collections.ObjectModel;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    public class TouchPanelBindingViewModel : ObservableObject
    {
        private MainOrchestrator _orchestrator;

        public TouchPanelBindingViewModel(MainOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
            PanelSelectCommand = new DelegateCommand(OnPanelSelected);
        }

        public DelegateCommand PanelSelectCommand { get; private set; }

        public ObservableCollection<PlaneProfile> PlaneProfiles { get { return _orchestrator.TouchPanel.PlaneProfiles; } }


        public void Initialize()
        {
            _orchestrator.TouchPanel.LoadPlaneProfiles();
        }

        private void OnPanelSelected()
        {
            _orchestrator.TouchPanel.PanelSelected(PlaneProfiles);
        }
    }
}
