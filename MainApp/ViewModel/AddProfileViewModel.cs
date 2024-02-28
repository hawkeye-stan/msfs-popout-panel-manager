using MaterialDesignThemes.Wpf;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class AddProfileViewModel : BaseViewModel
    {
        private readonly ProfileOrchestrator _profileOrchestrator;
        private readonly PanelSourceOrchestrator _panelSourceOrchestrator;

        public UserProfile Profile { get; set; }

        public UserProfile CopiedProfile { get; set; }

        public AddProfileViewModel(SharedStorage sharedStorage, ProfileOrchestrator profileOrchestrator, PanelSourceOrchestrator panelSourceOrchestrator) : base(sharedStorage)
        {
            _profileOrchestrator = profileOrchestrator;
            _panelSourceOrchestrator = panelSourceOrchestrator;
            
            Profile = new UserProfile();
        }

        public void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if (eventArgs.Parameter == null || !eventArgs.Parameter.Equals("ADD"))
                return;

            if (string.IsNullOrEmpty(Profile.Name.Trim()))
            {
                Profile.Name = null;
                eventArgs.Cancel();
                return;
            }

            // Unload the current profile
            ProfileData.ResetActiveProfile();

            _panelSourceOrchestrator.CloseAllPanelSource();
            _profileOrchestrator.AddProfile(Profile.Name, CopiedProfile);
        }
    }

    public class ProfileNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) => ValidationResult.ValidResult; // not used

        public override ValidationResult Validate(object value, CultureInfo cultureInfo, BindingExpressionBase owner)
        {
            var viewModel = (AddProfileViewModel)((BindingExpression)owner).DataItem;

            if (viewModel.ProfileData != null && viewModel.ProfileData.Profiles.Any(p => p.Name.ToLower().Equals(((string)value).ToLower())))
                return new ValidationResult(false, "Profile name already exist");

            if (string.IsNullOrEmpty(((string)value).Trim()))
                return new ValidationResult(false, "Profile name is required");

            return ValidationResult.ValidResult;
        }
    }
}
