using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using Prism.Commands;
using System;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class PopOutPanelSourceLegacyCardViewModel : BaseViewModel
    {
        private readonly PanelSourceOrchestrator _panelSourceOrchestrator;
        private readonly PanelConfigurationOrchestrator _panelConfigurationOrchestrator;

        public PanelConfig DataItem { get; set; }

        public ICommand AddPanelSourceLocationCommand { get; set; }

        public DelegateCommand<string> PanelAttributeUpdatedCommand { get; set; }

        public PopOutPanelSourceLegacyCardViewModel(SharedStorage sharedStorage, PanelSourceOrchestrator panelSourceOrchestrator, PanelConfigurationOrchestrator panelConfigurationOrchestrator) : base(sharedStorage)
        {
            _panelSourceOrchestrator = panelSourceOrchestrator;
            _panelConfigurationOrchestrator = panelConfigurationOrchestrator;

            AddPanelSourceLocationCommand = new DelegateCommand(OnAddPanelSourceLocation, () => ActiveProfile != null && !ActiveProfile.IsSelectingPanelSource && FlightSimData.IsInCockpit)
                                                                                    .ObservesProperty(() => ActiveProfile)
                                                                                    .ObservesProperty(() => ActiveProfile.IsSelectingPanelSource)
                                                                                    .ObservesProperty(() => FlightSimData.IsInCockpit);

            PanelAttributeUpdatedCommand = new DelegateCommand<string>(OnPanelAttributeUpdated);
        }

        private void OnPanelAttributeUpdated(string commandParameter)
        {
            if (DataItem != null && commandParameter != null)
                _panelConfigurationOrchestrator.PanelConfigPropertyUpdated(DataItem.PanelHandle, (PanelConfigPropertyName)Enum.Parse(typeof(PanelConfigPropertyName), commandParameter));
        }

        private void OnAddPanelSourceLocation()
        {
            _panelSourceOrchestrator.StartPanelSelectionEvent();

            DataItem.IsSelectedPanelSource = true;

            _panelSourceOrchestrator.StartPanelSelection(DataItem);
        }
    }
}
