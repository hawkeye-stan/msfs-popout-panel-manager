using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.UIController;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager.UI
{
    public partial class UserControlPanelConfiguration : UserControl, IPanelConfigurationView
    {
        private PanelConfigurationController _controller;

        public bool IsPanelLocked { set => SetProfileLockButtonText(_controller.DataStore.ActiveUserProfile.IsLocked); }

        public bool IsPanelChangeDisabled { set => this.Enabled = !value; }

        public UserControlPanelConfiguration()
        {
            InitializeComponent();
        }

        public void Initialize(PanelConfigurationController controller)
        {
            _controller = controller;

            _controller.RefreshDataUI += (source, e) => dataGridViewPanels.Refresh();
            _controller.HightlightSelectedPanel += HandleHighlightSelectedPanel;

            dataGridViewPanels.AutoGenerateColumns = false;
            dataGridViewPanels.AutoSize = false;
            dataGridViewPanels.DataSource = _controller.DataStore.PanelConfigs;
            dataGridViewPanels.CellValidating += HandleCellValidating;
            dataGridViewPanels.CellEndEdit += HandleCellValueChanged;
            dataGridViewPanels.CellContentClick += HandleCellContentChanged;        // for checkbox columns
            dataGridViewPanels.CellFormatting += HandleCellFormatting;

            buttonPixelPlusLarge.Click += (source, e) => HandleCellValueIncrDecr(10);
            buttonPixelPlusSmall.Click += (source, e) => HandleCellValueIncrDecr(1);
            buttonPixelLargeMinus.Click += (source, e) => HandleCellValueIncrDecr(-10);
            buttonPixelMinusSmall.Click += (source, e) => HandleCellValueIncrDecr(-1);

            buttonLockPanel.Click += HandleLockPanelChanged;
        }

        private void HandleCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                var column = (PanelConfigDataColumn)Enum.Parse(typeof(PanelConfigDataColumn), dataGridViewPanels.Columns[e.ColumnIndex].Name);

                var dgv = sender as DataGridView;
                var data = dgv.Rows[e.RowIndex].DataBoundItem as PanelConfig;

                if (column == PanelConfigDataColumn.PanelName || column == PanelConfigDataColumn.HideTitlebar)
                {
                    if(data.PanelType == PanelType.BuiltInPopout)
                        dgv[e.ColumnIndex, e.RowIndex].ReadOnly = true;
                }
            }
        }

        private void HandleCellValueIncrDecr(int changedAmount)
        {
            var activeCell = dataGridViewPanels.CurrentCell;

            if (activeCell != null)
            {
                var rowIndex = dataGridViewPanels.CurrentCell.RowIndex;
                var column = (PanelConfigDataColumn)Enum.Parse(typeof(PanelConfigDataColumn), dataGridViewPanels.Columns[dataGridViewPanels.CurrentCell.ColumnIndex].Name);
                _controller.CellValueIncrDecr(rowIndex, column, changedAmount);
            }
        }

        private void HandleCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.ColumnIndex <= 4 && e.RowIndex >= 0)
            {
                dataGridViewPanels.EndEdit();
                var column = (PanelConfigDataColumn)Enum.Parse(typeof(PanelConfigDataColumn), dataGridViewPanels.Columns[e.ColumnIndex].Name);
                _controller.CellValueChanged(e.RowIndex, column, dataGridViewPanels[e.ColumnIndex, e.RowIndex].EditedFormattedValue);
            }
        }

        private void HandleCellContentChanged(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex == 5 || e.ColumnIndex == 6) && e.RowIndex >= 0)
            {
                var column = (PanelConfigDataColumn)Enum.Parse(typeof(PanelConfigDataColumn), dataGridViewPanels.Columns[e.ColumnIndex].Name);
                _controller.CellValueChanged(e.RowIndex, column, dataGridViewPanels[e.ColumnIndex, e.RowIndex].EditedFormattedValue);
            }
        }

        private void HandleCellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                var column = (PanelConfigDataColumn)Enum.Parse(typeof(PanelConfigDataColumn), dataGridViewPanels.Columns[e.ColumnIndex].Name);

                // Allow cell edit for only specific data type in each column
                switch (column)
                {
                    case PanelConfigDataColumn.PanelName:
                        if (!(e.FormattedValue is String))
                            e.Cancel = true;
                        break;
                    case PanelConfigDataColumn.Left:
                    case PanelConfigDataColumn.Top:
                    case PanelConfigDataColumn.Width:
                    case PanelConfigDataColumn.Height:
                        // must be numbers
                        int i;
                        bool result = int.TryParse(Convert.ToString(e.FormattedValue), out i);

                        if (!result)
                            e.Cancel = true;
                        break;
                    default:
                        return;
                }
            }
        }

        private void HandleHighlightSelectedPanel(object sender, EventArgs<int> e)
        {
            dataGridViewPanels.ClearSelection();
            dataGridViewPanels.Rows[e.Value].Selected = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!_controller.DataStore.ActiveUserProfile.IsLocked)
            {
                if (keyData == (Keys.Oemplus | Keys.Control) || keyData == (Keys.Add | Keys.Control))
                {
                    HandleCellValueIncrDecr(10);
                }
                else if (keyData == (Keys.OemMinus | Keys.Control) || keyData == (Keys.Subtract | Keys.Control))
                {
                    HandleCellValueIncrDecr(-10);
                }
                else if (keyData == (Keys.OemCloseBrackets | Keys.Control))
                {
                    HandleCellValueIncrDecr(1);
                }
                else if (keyData == (Keys.OemOpenBrackets | Keys.Control))
                {
                    HandleCellValueIncrDecr(-1);
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void HandleLockPanelChanged(object sender, EventArgs e)
        {
            if (!_controller.DataStore.ActiveUserProfile.IsLocked)
            {
                _controller.LockPanelChanged(true);
            }
            else
            {
                var title = "Confirm Unlock Panels";
                var message = "Are you sure you want to unlock all panels to make changes?";

                using (var form = new ConfirmDialogForm(title, message) { StartPosition = FormStartPosition.CenterParent })
                {
                    if (form.ShowDialog() == DialogResult.Yes)
                    {
                        _controller.LockPanelChanged(false);
                    }
                }
            }
        }

        private void SetProfileLockButtonText(bool isLocked)
        {
            buttonLockPanel.Text = isLocked ? "Unlock Panels" : "Lock Panels";
            buttonLockPanel.BackColor = isLocked ? Color.Red : Color.FromArgb(17, 158, 218);

            dataGridViewPanels.ReadOnly = isLocked;
            buttonPixelLargeMinus.Enabled = !isLocked;
            buttonPixelMinusSmall.Enabled = !isLocked;
            buttonPixelPlusLarge.Enabled = !isLocked;
            buttonPixelPlusSmall.Enabled = !isLocked;
        }
    }
}
