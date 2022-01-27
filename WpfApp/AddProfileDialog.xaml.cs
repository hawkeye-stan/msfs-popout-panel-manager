using MahApps.Metro.Controls;
using MSFSPopoutPanelManager.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.WpfApp
{
    /// <summary>
    /// Interaction logic for AddProfileDialog.xaml
    /// </summary>
    public partial class AddProfileDialog : MetroWindow
    {
        public List<UserProfile> UserProfiles { get; set; }

        public int SelectedCopyProfileId { get; set; }

        public AddProfileDialog(ObservableCollection<UserProfile> userProfiles)
        {
            InitializeComponent();

            UserProfiles = userProfiles.ToList();
            UserProfiles.Insert(0, new UserProfile() { ProfileId = -1 });
            this.DataContext = this;

            SelectedCopyProfileId = -1;
            btnDialogOk.IsEnabled = false;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string ProfileName
        {
            get { return txtProfileName.Text; }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            txtProfileName.Focus();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            bool isNumber = e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9;
            bool isLetter = e.Key >= Key.A && e.Key <= Key.Z || (e.Key >= Key.A && e.Key <= Key.Z && e.KeyboardDevice.Modifiers == ModifierKeys.Shift);
            bool isCtrlA = e.Key == Key.A && e.KeyboardDevice.Modifiers == ModifierKeys.Control;
            bool isCtrlV = e.Key == Key.V && e.KeyboardDevice.Modifiers == ModifierKeys.Control;
            bool isBack = e.Key == Key.Back;
            bool isLeftOrRight = e.Key == Key.Left || e.Key == Key.Right;
            bool isEnterOrCancel = e.Key == Key.Enter || e.Key == Key.Escape;

            if (isNumber || isLetter || isCtrlA || isCtrlV || isBack || isLeftOrRight || isEnterOrCancel)
                e.Handled = false;
            else
                e.Handled = true;
        }

        private void txtProfileName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            btnDialogOk.IsEnabled = txtProfileName.Text.Length > 0;
        }
    }
}
