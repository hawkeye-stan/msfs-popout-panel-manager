using MSFSPopoutPanelManager.Orchestration;
using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class TrayIconViewModel : BaseViewModel
    {
        private readonly AppOrchestrator _appOrchestrator;
        private readonly PanelPopOutOrchestrator _panelPopOutOrchestrator;

        public DelegateCommand<object> ChangeProfileCommand { get; }

        public ICommand StartPopOutCommand { get; }

        public ICommand ExitAppCommand { get; }

        public TrayIconViewModel(SharedStorage sharedStorage, AppOrchestrator appOrchestrator, PanelPopOutOrchestrator panelPopOutOrchestrator) : base(sharedStorage)
        {
            _appOrchestrator = appOrchestrator;
            _panelPopOutOrchestrator = panelPopOutOrchestrator;

            StartPopOutCommand = new DelegateCommand(OnStartPopOut, () => ProfileData != null && ActiveProfile != null && ActiveProfile.PanelConfigs.Count > 0 && !ActiveProfile.HasUnidentifiedPanelSource && !ActiveProfile.IsEditingPanelSource && FlightSimData.IsInCockpit)
                                                                            .ObservesProperty(() => ActiveProfile)
                                                                            .ObservesProperty(() => ActiveProfile.PanelConfigs.Count)
                                                                            .ObservesProperty(() => ActiveProfile.HasUnidentifiedPanelSource)
                                                                            .ObservesProperty(() => ActiveProfile.IsEditingPanelSource)
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

        private async void OnStartPopOut()
        {
            await _panelPopOutOrchestrator.ManualPopOut();
        }

        private void OnExitApp()
        {
            _appOrchestrator.ApplicationClose();

            if (Application.Current != null)
                Environment.Exit(0);
        }
    }
}
