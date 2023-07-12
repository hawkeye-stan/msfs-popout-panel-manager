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
        public UserProfile Profile { get; set; }

        public UserProfile CopiedProfile { get; set; }

        public AddProfileViewModel(MainOrchestrator orchestrator) : base(orchestrator)
        {
            Profile = new UserProfile();
        }

        public void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if (eventArgs.Parameter != null && eventArgs.Parameter.Equals("ADD"))
            {
                if (string.IsNullOrEmpty(Profile.Name.Trim()))
                {
                    Profile.Name = null;
                    eventArgs.Cancel();
                    return;
                }

                // Unload the current profile
                ProfileData.ResetActiveProfile();

                Orchestrator.PanelSource.CloseAllPanelSource();
                Orchestrator.Profile.AddProfile(Profile.Name, CopiedProfile);
            }
        }
    }

    public class PostiveValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) => ValidationResult.ValidResult; // not used

        public override ValidationResult Validate(object value, CultureInfo cultureInfo, BindingExpressionBase owner)
        {
            var viewModel = (AddProfileViewModel)((BindingExpression)owner).DataItem;

            if (viewModel.ProfileData != null && viewModel.ProfileData.Profiles.Any(p => p.Name.ToLower() == ((string)value).ToLower()))
                return new ValidationResult(false, "Profile name already exist");

            if (string.IsNullOrEmpty(((string)value).Trim()))
                return new ValidationResult(false, "Profile name is required");

            return ValidationResult.ValidResult;
        }
    }
}
