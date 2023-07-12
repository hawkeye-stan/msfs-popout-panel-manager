using MaterialDesignThemes.Wpf;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.WindowsAgent;
using Prism.Commands;
using System;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class HelpViewModel : BaseViewModel
    {
        public DelegateCommand<string> HyperLinkCommand { get; private set; }

        public ICommand DeleteAppCacheCommand { get; private set; }

        public ICommand RollBackCommand { get; private set; }

        public string ApplicationVersion { get; private set; }

        public bool IsRollBackCommandVisible { get; private set; }

        public bool HasOrphanAppCache { get; private set; }

        public HelpViewModel(MainOrchestrator orchestrator) : base(orchestrator)
        {
            HyperLinkCommand = new DelegateCommand<string>(OnHyperLinkActivated);
            DeleteAppCacheCommand = new DelegateCommand(OnDeleteAppCache);
            RollBackCommand = new DelegateCommand(OnRollBack);
            ApplicationVersion = WindowProcessManager.GetApplicationVersion();

            IsRollBackCommandVisible = Orchestrator.Help.IsRollBackUpdateEnabled();
            HasOrphanAppCache = Orchestrator.Help.HasOrphanAppCache();
        }

        private void OnHyperLinkActivated(string commandParameter)
        {
            switch (commandParameter)
            {
                case "Getting Started":
                    Orchestrator.Help.OpenGettingStarted();
                    break;
                case "User Guide":
                    Orchestrator.Help.OpenUserGuide();
                    break;
                case "Download Latest GitHub":
                    Orchestrator.Help.OpenLatestDownloadGitHub();
                    break;
                case "Download Latest FlightsimTo":
                    Orchestrator.Help.OpenLatestDownloadFligthsimTo();
                    break;
                case "License":
                    Orchestrator.Help.OpenLicense();
                    break;
                case "Version Info":
                    Orchestrator.Help.OpenVersionInfo();
                    break;
                case "Open Data Folder":
                    Orchestrator.Help.OpenDataFolder();
                    break;
                case "Download VCC Library":
                    Orchestrator.Help.DownloadVCCLibrary();
                    break;
            }
        }

        private void OnDeleteAppCache()
        {
            Orchestrator.Help.DeleteAppCache();
            HasOrphanAppCache = Orchestrator.Help.HasOrphanAppCache();
        }

        private async void OnRollBack()
        {
            var result = await DialogHost.Show(new ConfirmationDialog($"WARNING!{Environment.NewLine}Are you sure you want to rollback to previous version of Pop Out Panel Manager (v3.4.6.0321)? All your changes since updated to v4.0.0 will be lost. Backups of user profile and application settings file from previous version of the application will be restored.", "Rollback"), "RootDialog");

            if (result != null && result.Equals("CONFIRM"))
            {
                Orchestrator.Help.RollBackUpdate();
            }
        }
    }
}
