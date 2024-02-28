using MaterialDesignThemes.Wpf;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using Prism.Commands;
using System;
using System.Windows.Input;
using MSFSPopoutPanelManager.MainApp.AppUserControl.Dialog;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class ProfileCardListViewModel : BaseViewModel
    {
        private readonly ProfileOrchestrator _profileOrchestrator;
        private readonly PanelSourceOrchestrator _panelSourceOrchestrator;

        public ICommand AddProfileCommand { get; }

        public ICommand NextProfileCommand { get; }

        public ICommand PreviousProfileCommand { get; }

        public ICommand SearchProfileSelectedCommand { get; set; }

        public UserProfile SearchProfileSelectedItem { get; set; }

        public int ProfileTransitionIndex { get; set; }

        public event EventHandler OnProfileSelected;

        public ProfileCardListViewModel(SharedStorage sharedStorage, ProfileOrchestrator profileOrchestrator, PanelSourceOrchestrator panelSourceOrchestrator) : base(sharedStorage)
        {
            _profileOrchestrator = profileOrchestrator;
            _panelSourceOrchestrator = panelSourceOrchestrator;

            AddProfileCommand = new DelegateCommand(OnAddProfile);
            NextProfileCommand = new DelegateCommand(OnNextProfile);
            PreviousProfileCommand = new DelegateCommand(OnPreviousProfile);
            SearchProfileSelectedCommand = new DelegateCommand(OnSearchProfileSelected);

            ProfileTransitionIndex = 0;
        }

        private async void OnAddProfile()
        {
            var dialog = new AddProfileDialog();
            var result = await DialogHost.Show(dialog, ROOT_DIALOG_HOST, null, dialog.ClosingEventHandler, null);

            if (result != null && result.ToString() == "ADD")
                UpdateProfileTransitionIndex();
        }

        private void OnNextProfile()
        {
            ProfileData.ResetActiveProfile();
            _panelSourceOrchestrator.CloseAllPanelSource();
            _profileOrchestrator.MoveToNextProfile();
            UpdateProfileTransitionIndex();
        }

        private void OnPreviousProfile()
        {
            ProfileData.ResetActiveProfile();
            _panelSourceOrchestrator.CloseAllPanelSource();
            _profileOrchestrator.MoveToPreviousProfile();
            UpdateProfileTransitionIndex();
        }

        private void OnSearchProfileSelected()
        {
            if (SearchProfileSelectedItem == null) 
                return;

            _profileOrchestrator.ChangeProfile(SearchProfileSelectedItem);
            UpdateProfileTransitionIndex();

            OnProfileSelected?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateProfileTransitionIndex()
        {
            ProfileTransitionIndex = ProfileTransitionIndex == 1 ? 0 : 1;
        }
    }
}
