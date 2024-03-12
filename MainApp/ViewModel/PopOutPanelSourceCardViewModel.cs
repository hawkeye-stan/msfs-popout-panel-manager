using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.Shared;
using Prism.Commands;
using System;
using System.Threading;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class PopOutPanelSourceCardViewModel : BaseViewModel
    {
        private readonly PanelSourceOrchestrator _panelSourceOrchestrator;
        private readonly PanelConfigurationOrchestrator _panelConfigurationOrchestrator;

        public PanelConfig DataItem { get; set; }

        public ICommand AddPanelSourceLocationCommand { get; set; }

        public DelegateCommand<string> PanelAttributeUpdatedCommand { get; set; }

        public DelegateCommand EditPanelSourceCommand { get; set; }
        
        public ObservableRangeCollection<FixedCameraConfig> FixedCameraConfigs { get; set; } = new();

        public PopOutPanelSourceCardViewModel(SharedStorage sharedStorage, PanelSourceOrchestrator panelSourceOrchestrator, PanelConfigurationOrchestrator panelConfigurationOrchestrator) : base(sharedStorage)
        {
            _panelSourceOrchestrator = panelSourceOrchestrator;
            _panelConfigurationOrchestrator = panelConfigurationOrchestrator;

            AddPanelSourceLocationCommand = new DelegateCommand(OnAddPanelSourceLocation, () => ActiveProfile != null && !ActiveProfile.IsSelectingPanelSource && FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => ActiveProfile)
                                                                                .ObservesProperty(() => ActiveProfile.IsSelectingPanelSource)
                                                                                .ObservesProperty(() => FlightSimData.IsInCockpit);

            EditPanelSourceCommand = new DelegateCommand(OnEditPanelSource, () => ActiveProfile != null && FlightSimData.IsInCockpit)
                                                                                .ObservesProperty(() => ActiveProfile)
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
            _panelSourceOrchestrator.ShowPanelSourceForEdit(DataItem);      // This is to reset active panel source

            FixedCameraConfigs.Clear();
            FixedCameraConfigs.AddRange(_panelSourceOrchestrator.GetFixedCameraConfigs());

            DataItem.IsEditingPanel = true;
            
            _panelSourceOrchestrator.StartPanelSelectionEvent();

            _panelSourceOrchestrator.StartPanelSelection(DataItem);
        }

        public void SetCamera()
        {
            _panelSourceOrchestrator.SetCamera(DataItem);
        }

        private void OnEditPanelSource()
        {
            if (!DataItem.HasPanelSource)
                return;

            FixedCameraConfigs.Clear();
            FixedCameraConfigs.AddRange(_panelSourceOrchestrator.GetFixedCameraConfigs());

            DataItem.IsSelectedPanelSource = true;
            _panelSourceOrchestrator.ShowPanelSourceForEdit(DataItem);
        }
    }
}
