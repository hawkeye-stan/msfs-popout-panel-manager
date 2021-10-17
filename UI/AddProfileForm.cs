using DarkUI.Forms;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager
{
    public partial class AddProfileForm : DarkForm
    {
        private PanelManager _panelManager;

        public event EventHandler<EventArgs<int>> OnAddProfile;

        public AddProfileForm(PanelManager panelManager)
        {
            InitializeComponent();

            _panelManager = panelManager;
            SetTemplateDropDown();
        }

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

        public void SetTemplateDropDown()
        {
            try
            {
                var templates = FileManager.ReadAllAnalysisTemplateData();

                var templateNames = templates.OrderBy(x => x.TemplateName).Select(x => x.TemplateName).Distinct().ToList();
                templateNames.Insert(0, "New");
                comboBoxTemplates.DataSource = templateNames;
            }
            catch (Exception ex)
            {
                Logger.LogStatus(ex.Message);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var profileName = textBoxProfileName.Text.Trim();
            var analysisTemplateName = Convert.ToString(comboBoxTemplates.SelectedValue);

            var profileId = _panelManager.AddUserProfile(profileName, analysisTemplateName);

            OnAddProfile?.Invoke(this, new EventArgs<int>(profileId));

            this.Close();
        }
    }
}
