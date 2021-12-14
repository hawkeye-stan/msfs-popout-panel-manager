using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.UIController;
using System;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager.UI
{
    public partial class UserControlPanelConfiguration : UserControl
    {
        private PanelConfigurationController _controller;

        public UserControlPanelConfiguration()
        {
            InitializeComponent();
            _controller = new PanelConfigurationController();

            _controller.RefreshDataUI += (source, e) => dataGridViewPanels.Refresh();
            _controller.HightlightSelectedPanel += HandleHighlightSelectedPanel;
            
            dataGridViewPanels.AutoGenerateColumns = false;
            dataGridViewPanels.AutoSize = false;
            dataGridViewPanels.DataSource = _controller.PanelConfigs;
            dataGridViewPanels.CellBeginEdit += HandleCellBeginEdit;
            dataGridViewPanels.CellValidating += HandleCellValidating;
            dataGridViewPanels.CellValueChanged += HandleCellValueChanged;
            dataGridViewPanels.CellContentClick += HandleCellValueChanged;

            buttonSaveSettings.Click += (source, e) => { dataGridViewPanels.EndEdit(); _controller.SaveSettings(); };
            buttonRestart.Click += (source, e) => _controller.BackToPanelSelection();

            buttonPixelPlusLarge.Click += (source, e) => HandleCellValueIncrDecr(10);
            buttonPixelPlusSmall.Click += (source, e) => HandleCellValueIncrDecr(1);
            buttonPixelMinusLarge.Click += (source, e) => HandleCellValueIncrDecr(-10);
            buttonPixelMinusSmall.Click += (source, e) => HandleCellValueIncrDecr(-1);
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
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                var column = (PanelConfigDataColumn)Enum.Parse(typeof(PanelConfigDataColumn), dataGridViewPanels.Columns[e.ColumnIndex].Name);
                _controller.CellValueChanged(e.RowIndex, column, dataGridViewPanels[e.ColumnIndex, e.RowIndex].EditedFormattedValue);
            }
        }

        private void HandleCellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {

                var column = (PanelConfigDataColumn)Enum.Parse(typeof(PanelConfigDataColumn), dataGridViewPanels.Columns[e.ColumnIndex].Name);

                // Disallow cell edit 
                var dgv = sender as DataGridView;
                var data = dgv.Rows[e.RowIndex].DataBoundItem as PanelConfig;

                if (column == PanelConfigDataColumn.PanelName || column == PanelConfigDataColumn.AlwaysOnTop || column == PanelConfigDataColumn.HideTitlebar)
                {
                    if (data.PanelType == PanelType.BuiltInPopout)
                        e.Cancel = true;
                }
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
    }
}
