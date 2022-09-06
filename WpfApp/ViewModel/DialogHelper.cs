using MSFSPopoutPanelManager.UserDataAgent;
using System.Collections.Generic;
using System.Windows;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    internal class DialogHelper
    {
        public static bool ConfirmDialog(string title, string message)
        {
            var dialog = new ConfirmationDialog(title, message);
            dialog.Owner = Application.Current.MainWindow;
            dialog.Topmost = true;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            return (bool)dialog.ShowDialog();
        }

        public static bool ConfirmDialog(string title, string message, Window owner)
        {
            var dialog = new ConfirmationDialog(title, message);
            dialog.Owner = owner;
            dialog.Topmost = true;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            return (bool)dialog.ShowDialog();
        }

        public static AddProfileDialogResult AddProfileDialog(List<Profile> profiles)
        {
            var dialog = new AddProfileDialog(profiles);
            dialog.Owner = Application.Current.MainWindow;
            dialog.Topmost = true;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if ((bool)dialog.ShowDialog())
            {
                return new AddProfileDialogResult(dialog.ProfileName, dialog.SelectedCopyProfileId);
            }

            return null;
        }

        public static void PreferencesDialog(PreferencesViewModel preferencesViewModel)
        {
            var dialog = new PreferencesDialog(preferencesViewModel);
            dialog.Owner = Application.Current.MainWindow;
            dialog.Topmost = true;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }

        public static void PanelConfigurationInstructionDialog()
        {
            var dialog = new PanelConfigurationInstructionDialog();
            dialog.Owner = Application.Current.MainWindow;
            dialog.Topmost = true;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }
    }

    internal class AddProfileDialogResult
    {
        public AddProfileDialogResult(string profileName, int copyPofileId)
        {
            ProfileName = profileName;
            CopyProfileId = copyPofileId;
        }

        public string ProfileName { get; set; }

        public int CopyProfileId { get; set; }
    }
}
