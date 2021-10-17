using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager
{
    public partial class UserControlApplySettings : UserControlCommon
    {
        private BindingList<PanelDestinationInfo> _panelInfoList;

        public UserControlApplySettings(PanelManager panelManager) : base(panelManager)
        {
            InitializeComponent();

            panelManager.OnAnalysisCompleted += PanelManager_OnAnalysisCompleted;
            panelManager.OnPanelSettingsChanged += PanelManager_OnAnalysisCompleted;
        }

        public event EventHandler OnRestart;

        private void PanelManager_OnAnalysisCompleted(object sender, EventArgs e)
        {
            _panelInfoList = new BindingList<PanelDestinationInfo>(PanelManager.CurrentPanelProfile.PanelSettings.PanelDestinationList.OrderBy(x => x.PanelName).ToList());

            dataGridViewPanels.AutoGenerateColumns = false;
            dataGridViewPanels.AutoSize = false;
            dataGridViewPanels.DataSource = _panelInfoList;

            checkBoxAlwaysOnTop.Checked = PanelManager.CurrentPanelProfile.PanelSettings.AlwaysOnTop;
            checkBoxHidePanelTitleBar.Checked = PanelManager.CurrentPanelProfile.PanelSettings.HidePanelTitleBar;

           
        }

        private void buttonRestart_Click(object sender, EventArgs e)
        {
            OnRestart?.Invoke(this, null);
            PanelManager.UpdatePanelLocationUI();
        }

        private void buttonApplySettings_Click(object sender, EventArgs e)
        {
            PanelManager.ApplyPanelSettings();
        }

        private void buttonSaveSettings_Click(object sender, EventArgs e)
        {
            PanelManager.SavePanelSettings();
        }

        private void checkBoxHidePanelTitleBar_CheckedChanged(object sender, EventArgs e)
        {
            PanelManager.CurrentPanelProfile.PanelSettings.HidePanelTitleBar = checkBoxHidePanelTitleBar.Checked;
        }

        private void checkBoxAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            PanelManager.CurrentPanelProfile.PanelSettings.AlwaysOnTop = checkBoxAlwaysOnTop.Checked;
        }

        private void dataGridViewPanels_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var panelName = Convert.ToString(dataGridViewPanels[0, e.RowIndex].FormattedValue);

            var left = Convert.ToInt32(dataGridViewPanels[1, e.RowIndex].FormattedValue);
            var top = Convert.ToInt32(dataGridViewPanels[2, e.RowIndex].FormattedValue);
            var width = Convert.ToInt32(dataGridViewPanels[3, e.RowIndex].FormattedValue);
            var height = Convert.ToInt32(dataGridViewPanels[4, e.RowIndex].FormattedValue);

            var panel = PanelManager.CurrentPanelProfile.PanelSettings.PanelDestinationList.Find(x => x.PanelName == panelName);

            PInvoke.MoveWindow(panel.PanelHandle, left, top, width, height, true);
        }

        private void dataGridViewPanels_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                DataGridView dgv = sender as DataGridView;
                PanelDestinationInfo data = dgv.Rows[e.RowIndex].DataBoundItem as PanelDestinationInfo;

                if(!data.IsOpened && data.PanelType == WindowType.Custom_Popout)
                {
                    dataGridViewPanels.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.PaleVioletRed;
                }                
            }
        }

        private void dataGridViewPanels_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // must be numbers
            if(e.ColumnIndex >= 1 && e.ColumnIndex <= 4)
            {
                int i = 0;
                bool result = int.TryParse(Convert.ToString(e.FormattedValue), out i);

                if (!result)
                    e.Cancel = true;
            }
        }
    }
}
