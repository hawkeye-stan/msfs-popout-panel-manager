using MaterialDesignThemes.Wpf;
using MSFSPopoutPanelManager.MainApp.AppUserControl.Dialog;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.WindowsAgent;
using Prism.Commands;
using System;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class HelpViewModel : BaseViewModel
    {
        private readonly HelpOrchestrator _helpOrchestrator;

        public DelegateCommand<string> HyperLinkCommand { get; private set; }

        public ICommand DeleteAppCacheCommand { get; private set; }

        public ICommand RollBackCommand { get; private set; }

        public string ApplicationVersion { get; private set; }

        public bool IsRollBackCommandVisible { get; private set; }

        public bool HasOrphanAppCache { get; private set; }

        public HelpViewModel(SharedStorage sharedStorage, HelpOrchestrator helpOrchestrator) : base(sharedStorage)
        {
            _helpOrchestrator = helpOrchestrator;

            HyperLinkCommand = new DelegateCommand<string>(OnHyperLinkActivated);
            DeleteAppCacheCommand = new DelegateCommand(OnDeleteAppCache);
            RollBackCommand = new DelegateCommand(OnRollBack);

#if DEBUG
            var buildConfig = " (Debug)";
#elif LOCAL
            var buildConfig = " (Local)";
#else
            var buildConfig = string.Empty;
#endif

            ApplicationVersion = $"{WindowProcessManager.GetApplicationVersion()}{buildConfig}";

            IsRollBackCommandVisible = _helpOrchestrator.IsRollBackUpdateEnabled();
            HasOrphanAppCache = _helpOrchestrator.HasOrphanAppCache();
        }

        private void OnHyperLinkActivated(string commandParameter)
        {
            switch (commandParameter)
            {
                case "Getting Started":
                    _helpOrchestrator.OpenGettingStarted();
                    break;
                case "User Guide":
                    _helpOrchestrator.OpenUserGuide();
                    break;
                case "Download Latest GitHub":
                    _helpOrchestrator.OpenLatestDownloadGitHub();
                    break;
                case "Download Latest FlightsimTo":
                    _helpOrchestrator.OpenLatestDownloadFligthsimTo();
                    break;
                case "License":
                    _helpOrchestrator.OpenLicense();
                    break;
                case "Version Info":
                    _helpOrchestrator.OpenVersionInfo();
                    break;
                case "Open Data Folder":
                    _helpOrchestrator.OpenDataFolder();
                    break;
                case "Download VCC Library":
                    _helpOrchestrator.DownloadVccLibrary();
                    break;
            }
        }

        private void OnDeleteAppCache()
        {
            _helpOrchestrator.DeleteAppCache();
            HasOrphanAppCache = _helpOrchestrator.HasOrphanAppCache();
        }

        private async void OnRollBack()
        {
            var result = await DialogHost.Show(new ConfirmationDialog($"WARNING!{Environment.NewLine}Are you sure you want to rollback to previous version of Pop Out Panel Manager (v3.4.6.0321)? All your changes since updated to v4.0.0 will be lost. Backups of user profile and application settings file from previous version of the application will be restored.", "Rollback"), "RootDialog");

            if (result != null && result.Equals("CONFIRM"))
            {
                _helpOrchestrator.RollBackUpdate();
            }
        }
    }
}
