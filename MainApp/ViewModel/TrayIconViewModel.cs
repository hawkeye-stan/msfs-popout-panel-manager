using MSFSPopoutPanelManager.Orchestration;
using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class TrayIconViewModel : BaseViewModel
    {
        public DelegateCommand<object> ChangeProfileCommand { get; }

        public ICommand StartPopOutCommand { get; }

        public ICommand ExitAppCommand { get; }

        public TrayIconViewModel(MainOrchestrator orchestrator) : base(orchestrator)
        {
            StartPopOutCommand = new DelegateCommand(OnStartPopOut, () => ProfileData != null && ProfileData.ActiveProfile != null && ProfileData.ActiveProfile.PanelConfigs.Count > 0 && !ProfileData.ActiveProfile.HasUnidentifiedPanelSource && !ProfileData.ActiveProfile.IsEditingPanelSource && FlightSimData.IsInCockpit)
                                                                            .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                            .ObservesProperty(() => ProfileData.ActiveProfile.PanelConfigs.Count)
                                                                            .ObservesProperty(() => ProfileData.ActiveProfile.HasUnidentifiedPanelSource)
                                                                            .ObservesProperty(() => ProfileData.ActiveProfile.IsEditingPanelSource)
                                                                            .ObservesProperty(() => FlightSimData.IsInCockpit);

            ChangeProfileCommand = new DelegateCommand<object>(OnChangeProfile);

            ExitAppCommand = new DelegateCommand(OnExitApp);
        }

        private void OnChangeProfile(object param)
        {
            var profileId = Convert.ToString(param);

            if (string.IsNullOrEmpty(profileId))
                return;

            ProfileData.ResetActiveProfile();
            ProfileData.SetActiveProfile(new Guid(profileId));
        }

        private void OnStartPopOut()
        {
            Orchestrator.PanelPopOut.ManualPopOut();
        }

        private void OnExitApp()
        {
            Orchestrator.ApplicationClose();

            if (Application.Current != null)
                Environment.Exit(0);
        }
    }
}
