using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.UIController;
using System;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager.UI
{
    public partial class UserControlPanelSelection : UserControl, IPanelSelectionView
    {
        private PanelSelectionController _controller;

        #region Implement view interface

        public Form Form { get => ParentForm; }

        public bool ShowPanelLocationOverlay { get => checkBoxShowPanelLocation.Checked; set => checkBoxShowPanelLocation.Checked = value; }

        #endregion

        public UserControlPanelSelection()
        {
            InitializeComponent();
        }

        public void Initialize(PanelSelectionController controller)
        {
            _controller = controller;
            _controller.OnUIStateChanged += HandleOnUIStateChanged;
            _controller.Initialize();

            // Set bindings
            comboBoxProfile.DisplayMember = "ProfileName";
            comboBoxProfile.ValueMember = "ProfileId";
            comboBoxProfile.DataSource = _controller.DataStore.UserProfiles;
            comboBoxProfile.DataBindings.Add("SelectedValue", _controller.DataStore, "ActiveUserProfileId");

            comboBoxProfile.SelectedValue = -1;     // forced a default
            comboBoxProfile.SelectedIndexChanged += HandleProfileChanged;

            buttonAddProfile.Click += HandleAddProfile;
            buttonDeleteProfile.Click += HandleDeleteProfile;
            buttonSetDefault.Click += (source, e) => _controller.SetDefaultProfile();
            buttonStartPanelSelection.Click += HandleStartPanelSelection;
            buttonStartPopOut.Click += (source, e) => _controller.StartPopOut();

            dataGridViewPanelCoor.AutoGenerateColumns = false;
            dataGridViewPanelCoor.AutoSize = false;
            dataGridViewPanelCoor.DataSource = _controller.DataStore.ActiveProfilePanelCoordinates;

            checkBoxShowPanelLocation.CheckedChanged += (source, e) => _controller.SetPanelLocationOverlayChanged();
        }


        private void HandleProfileChanged(object sender, EventArgs e)
        {
            if(Convert.ToInt32(comboBoxProfile.SelectedValue) > 0)
                _controller.ProfileChanged(Convert.ToInt32(comboBoxProfile.SelectedValue));
        }

        private void HandleAddProfile(object sender, EventArgs e)
        {
            using (var form = new AddProfileForm { StartPosition = FormStartPosition.CenterParent })
            {
                var dialogResult = form.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    _controller.AddProfile(form.ProfileName);
                }
            }
        }

        private void HandleDeleteProfile(object sender, EventArgs e)
        {
            var title = "Confirm Delete";
            var message = "Are you sure you want to delete the selected profile?";

            using (var form = new ConfirmDialogForm(title, message) { StartPosition = FormStartPosition.CenterParent })
            {
                if (form.ShowDialog() == DialogResult.Yes)
                    _controller.DeleteProfile();
            }
        }

        private void HandleStartPanelSelection(object sender, EventArgs e)
        {
            if (!_controller.HasExistingPanelCoordinates)
            {
                _controller.StartPanelSelection();
            }
            else
            {
                var title = "Confirm Overwrite";
                var message = "WARNING! Are you sure you want to overwrite existing saved panel locations and ALL settings for this profile?";

                using (var form = new ConfirmDialogForm(title, message) { StartPosition = FormStartPosition.CenterParent })
                {
                    if (form.ShowDialog() == DialogResult.Yes)
                        _controller.StartPanelSelection();
                }
            }
        }

        private void HandleOnUIStateChanged(object sender, EventArgs<PanelSelectionUIState> e)
        {
            switch (e.Value)
            {
                case PanelSelectionUIState.NoProfileSelected:
                    comboBoxProfile.Enabled = true;
                    buttonAddProfile.Enabled = true;
                    buttonDeleteProfile.Enabled = false;
                    buttonSetDefault.Enabled = false;
                    buttonStartPanelSelection.Enabled = false;
                    checkBoxShowPanelLocation.Enabled = false;
                    buttonStartPopOut.Enabled = false;
                    break;
                case PanelSelectionUIState.ProfileSelected:
                    comboBoxProfile.Enabled = true;
                    buttonAddProfile.Enabled = true;
                    buttonDeleteProfile.Enabled = true;
                    buttonSetDefault.Enabled = true;
                    buttonStartPanelSelection.Enabled = true;
                    checkBoxShowPanelLocation.Enabled = true;
                    buttonStartPopOut.Enabled = false;
                    break;
                case PanelSelectionUIState.PanelSelectionStarted:
                    comboBoxProfile.Enabled = true;
                    buttonAddProfile.Enabled = false;
                    buttonDeleteProfile.Enabled = false;
                    buttonSetDefault.Enabled = false;
                    buttonStartPanelSelection.Enabled = false;
                    checkBoxShowPanelLocation.Enabled = false;
                    buttonStartPopOut.Enabled = false;
                    break;
                case PanelSelectionUIState.PanelSelectionCompletedValid:
                    comboBoxProfile.Enabled = true;
                    buttonAddProfile.Enabled = true;
                    buttonDeleteProfile.Enabled = true;
                    buttonSetDefault.Enabled = true;
                    buttonStartPanelSelection.Enabled = true;
                    checkBoxShowPanelLocation.Enabled = true;
                    buttonStartPopOut.Enabled = true;
                    buttonStartPopOut.Focus();
                    break;
                case PanelSelectionUIState.PanelSelectionCompletedInvalid:
                    comboBoxProfile.Enabled = true;
                    buttonAddProfile.Enabled = true;
                    buttonDeleteProfile.Enabled = true;
                    buttonSetDefault.Enabled = true;
                    buttonStartPanelSelection.Enabled = true;
                    checkBoxShowPanelLocation.Enabled = true;
                    buttonStartPopOut.Enabled = false;
                    break;
                case PanelSelectionUIState.PopoutStarted:
                    comboBoxProfile.Enabled = false;
                    buttonAddProfile.Enabled = false;
                    buttonDeleteProfile.Enabled = false;
                    buttonSetDefault.Enabled = false;
                    buttonStartPanelSelection.Enabled = false;
                    checkBoxShowPanelLocation.Enabled = false;
                    buttonStartPopOut.Enabled = false;
                    break;
            }
        }
    }
}
