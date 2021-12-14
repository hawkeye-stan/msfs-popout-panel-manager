using DarkUI.Forms;
using MSFSPopoutPanelManager.UIController;
using System;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager.UI
{
    public partial class AddProfileForm : DarkForm
    {
        public AddProfileForm()
        {
            InitializeComponent();
        }

        public string ProfileName { get { return textBoxProfileName.Text.Trim(); } }

        private void textBoxProfileName_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !(Char.IsLetterOrDigit(e.KeyChar) ||
                          Char.IsPunctuation(e.KeyChar) ||
                          e.KeyChar == (char)Keys.Space ||
                          e.KeyChar == (char)Keys.Back);
        }

        private void textBoxProfileName_TextChanged(object sender, EventArgs e)
        {
            buttonOK.Enabled = textBoxProfileName.Text.Trim().Length > 0;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}