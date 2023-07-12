using MaterialDesignThemes.Wpf;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using Prism.Commands;
using System;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class ProfileCardListViewModel : BaseViewModel
    {
        public ICommand AddProfileCommand { get; }

        public ICommand NextProfileCommand { get; }

        public ICommand PreviousProfileCommand { get; }

        public ICommand SearchProfileSelectedCommand { get; set; }

        public UserProfile SearchProfileSelectedItem { get; set; }

        public int ProfileTransitionIndex { get; set; }

        public bool IsProfileListEmpty { get { return ProfileData?.Profiles.Count == 0; } }

        public bool IsProfileListNotEmpty { get { return ProfileData?.Profiles.Count > 0; } }

        public event EventHandler OnProfileSelected;

        public ProfileCardListViewModel(MainOrchestrator orchestrator) : base(orchestrator)
        {
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

            if (result.ToString() == "ADD")
                UpdateProfileTransitionIndex();
        }

        private void OnNextProfile()
        {
            ProfileData.ResetActiveProfile();
            Orchestrator.PanelSource.CloseAllPanelSource();
            Orchestrator.Profile.MoveToNextProfile();
            UpdateProfileTransitionIndex();
        }

        private void OnPreviousProfile()
        {
            ProfileData.ResetActiveProfile();
            Orchestrator.PanelSource.CloseAllPanelSource();
            Orchestrator.Profile.MoveToPreviousProfile();
            UpdateProfileTransitionIndex();
        }

        private void OnSearchProfileSelected()
        {
            if (SearchProfileSelectedItem != null)
            {
                Orchestrator.Profile.ChangeProfile(SearchProfileSelectedItem);
                UpdateProfileTransitionIndex();

                OnProfileSelected?.Invoke(this, null);
            }
        }

        private void UpdateProfileTransitionIndex()
        {
            ProfileTransitionIndex = ProfileTransitionIndex == 1 ? 0 : 1;
        }
    }
}
