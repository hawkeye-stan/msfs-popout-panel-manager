using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.UIController;
using System;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager.UI
{
    public partial class UserControlPanelSelection : UserControl
    {
        private PanelSelectionController _controller;

        public UserControlPanelSelection()
        {
            InitializeComponent();
            _controller = new PanelSelectionController();

            // Listen to controller event
            _controller.OnUIStateChanged += HandleOnUIStateChanged;
            _controller.Initialize();

            // Set bindings
            comboBoxProfile.DisplayMember = "ProfileName";
            comboBoxProfile.ValueMember = "ProfileId";
            comboBoxProfile.DataSource = _controller.PlaneProfileList;
            comboBoxProfile.DataBindings.Add("SelectedValue", _controller, "SelectedProfileId");
            comboBoxProfile.SelectedValue = -1;     // forced a default
            comboBoxProfile.SelectedIndexChanged += HandleProfileChanged;

            buttonAddProfile.Click += HandleAddProfile;
            buttonDeleteProfile.Click += HandleDeleteProfile;
            buttonSetDefault.Click += (source, e) => _controller.SetDefaultProfile();
            buttonPanelSelection.Click += HandlePanelSelectionStarted;
            buttonStartPopOut.Click += (source, e) => _controller.StartPopOut(ParentForm);

            dataGridViewPanelCoor.AutoGenerateColumns = false;
            dataGridViewPanelCoor.AutoSize = false;
            dataGridViewPanelCoor.DataSource = _controller.PanelCoordinates;

            checkBoxShowPanelLocation.DataBindings.Add("Checked", _controller, "ShowPanelLocationOverlay");
            checkBoxShowPanelLocation.CheckedChanged += (source, e) => _controller.ShowPanelLocationOverlayChanged(checkBoxShowPanelLocation.Checked);
        }

        private void HandleAddProfile(object sender, EventArgs e)
        {
            using(var form = new AddProfileForm { StartPosition = FormStartPosition.CenterParent })
            {
                var dialogResult = form.ShowDialog();

                if(dialogResult == DialogResult.OK)
                {
                    _controller.AddUserProfile(form.ProfileName);
                }
            }
        }

        private void HandleDeleteProfile(object sender, EventArgs e)
        {
            var title = "Confirm Delete";
            var message = "Are you sure you want to delete the selected profile?";

            using (var form = new ConfirmDialogForm(title, message) { StartPosition = FormStartPosition.CenterParent })
            {
                var dialogResult = form.ShowDialog();

                if (dialogResult == DialogResult.Yes)
                {
                    _controller.DeleteProfile();
                }
            }
        }

        private void HandleProfileChanged(object sender, EventArgs e)
        {
            if(Convert.ToInt32(comboBoxProfile.SelectedValue) > 0)
                _controller.ProfileChanged(Convert.ToInt32(comboBoxProfile.SelectedValue));
        }

        private void HandlePanelSelectionStarted(object sender, EventArgs e)
        {
            if (_controller.ActiveProfile != null)
            {
                if (_controller.ActiveProfile.PanelConfigs.Count > 0)
                {
                    var title = "Confirm Overwrite";
                    var message = "Are you sure you want to overwrite existing saved panel locations and settings for this profile??";

                    using (var form = new ConfirmDialogForm(title, message) { StartPosition = FormStartPosition.CenterParent })
                    {
                        var dialogResult = form.ShowDialog();

                        if (dialogResult == DialogResult.No)
                        {
                            return;
                        }
                    }
                }

                _controller.StartPanelSelection(ParentForm);
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
                    buttonPanelSelection.Enabled = false;
                    checkBoxShowPanelLocation.Enabled = false;
                    buttonStartPopOut.Enabled = false;
                    break;
                case PanelSelectionUIState.ProfileSelected:
                    comboBoxProfile.Enabled = true;
                    buttonAddProfile.Enabled = true;
                    buttonDeleteProfile.Enabled = true;
                    buttonSetDefault.Enabled = true;
                    buttonPanelSelection.Enabled = true;
                    checkBoxShowPanelLocation.Enabled = true;
                    buttonStartPopOut.Enabled = false;
                    break;
                case PanelSelectionUIState.PanelSelectionStarted:
                    comboBoxProfile.Enabled = true;
                    buttonAddProfile.Enabled = false;
                    buttonDeleteProfile.Enabled = false;
                    buttonSetDefault.Enabled = false;
                    buttonPanelSelection.Enabled = false;
                    checkBoxShowPanelLocation.Enabled = false;
                    buttonStartPopOut.Enabled = false;
                    break;
                case PanelSelectionUIState.PanelSelectionCompletedValid:
                    comboBoxProfile.Enabled = true;
                    buttonAddProfile.Enabled = true;
                    buttonDeleteProfile.Enabled = true;
                    buttonSetDefault.Enabled = true;
                    buttonPanelSelection.Enabled = true;
                    checkBoxShowPanelLocation.Enabled = true;
                    buttonStartPopOut.Enabled = true;
                    buttonStartPopOut.Focus();
                    break;
                case PanelSelectionUIState.PanelSelectionCompletedInvalid:
                    comboBoxProfile.Enabled = true;
                    buttonAddProfile.Enabled = true;
                    buttonDeleteProfile.Enabled = true;
                    buttonSetDefault.Enabled = true;
                    buttonPanelSelection.Enabled = true;
                    checkBoxShowPanelLocation.Enabled = true;
                    buttonStartPopOut.Enabled = false;
                    break;
                case PanelSelectionUIState.PopoutStarted:
                    comboBoxProfile.Enabled = false;
                    buttonAddProfile.Enabled = false;
                    buttonDeleteProfile.Enabled = false;
                    buttonSetDefault.Enabled = false;
                    buttonPanelSelection.Enabled = false;
                    checkBoxShowPanelLocation.Enabled = false;
                    buttonStartPopOut.Enabled = false;
                    break;
            }
        }
    }
}
