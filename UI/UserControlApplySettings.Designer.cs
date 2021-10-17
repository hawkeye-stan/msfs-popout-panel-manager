
namespace MSFSPopoutPanelManager
{
    partial class UserControlApplySettings
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridViewPanels = new System.Windows.Forms.DataGridView();
            this.PanelName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Left = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Top = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Width = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Height = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonRestart = new System.Windows.Forms.Button();
            this.checkBoxAlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.checkBoxHidePanelTitleBar = new System.Windows.Forms.CheckBox();
            this.buttonApplySettings = new System.Windows.Forms.Button();
            this.buttonSaveSettings = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPanels)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.dataGridViewPanels);
            this.panel1.Controls.Add(this.label2);
            this.panel1.ForeColor = System.Drawing.Color.White;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(860, 277);
            this.panel1.TabIndex = 1;
            // 
            // dataGridViewPanels
            // 
            this.dataGridViewPanels.AllowUserToAddRows = false;
            this.dataGridViewPanels.AllowUserToDeleteRows = false;
            this.dataGridViewPanels.AllowUserToResizeColumns = false;
            this.dataGridViewPanels.AllowUserToResizeRows = false;
            this.dataGridViewPanels.CausesValidation = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewPanels.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewPanels.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPanels.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PanelName,
            this.Left,
            this.Top,
            this.Width,
            this.Height});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewPanels.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewPanels.Location = new System.Drawing.Point(20, 35);
            this.dataGridViewPanels.MultiSelect = false;
            this.dataGridViewPanels.Name = "dataGridViewPanels";
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewPanels.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridViewPanels.RowHeadersVisible = false;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.Padding = new System.Windows.Forms.Padding(3);
            this.dataGridViewPanels.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridViewPanels.RowTemplate.Height = 25;
            this.dataGridViewPanels.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridViewPanels.ShowCellErrors = false;
            this.dataGridViewPanels.ShowCellToolTips = false;
            this.dataGridViewPanels.ShowEditingIcon = false;
            this.dataGridViewPanels.ShowRowErrors = false;
            this.dataGridViewPanels.Size = new System.Drawing.Size(820, 225);
            this.dataGridViewPanels.TabIndex = 8;
            this.dataGridViewPanels.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewPanels_CellEndEdit);
            this.dataGridViewPanels.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridViewPanels_CellFormatting);
            this.dataGridViewPanels.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dataGridViewPanels_CellValidating);
            // 
            // PanelName
            // 
            this.PanelName.DataPropertyName = "PanelName";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.PanelName.DefaultCellStyle = dataGridViewCellStyle2;
            this.PanelName.HeaderText = "Panel Name";
            this.PanelName.Name = "PanelName";
            this.PanelName.ReadOnly = true;
            this.PanelName.Width = 355;
            // 
            // Left
            // 
            this.Left.DataPropertyName = "Left";
            this.Left.HeaderText = "X Pos";
            this.Left.MaxInputLength = 6;
            this.Left.Name = "Left";
            this.Left.Width = 115;
            // 
            // Top
            // 
            this.Top.DataPropertyName = "Top";
            this.Top.HeaderText = "Y Pos";
            this.Top.MaxInputLength = 6;
            this.Top.Name = "Top";
            this.Top.Width = 115;
            // 
            // Width
            // 
            this.Width.DataPropertyName = "Width";
            this.Width.HeaderText = "Width";
            this.Width.MaxInputLength = 6;
            this.Width.Name = "Width";
            this.Width.Width = 115;
            // 
            // Height
            // 
            this.Height.DataPropertyName = "Height";
            this.Height.HeaderText = "Height";
            this.Height.MaxInputLength = 6;
            this.Height.Name = "Height";
            this.Height.Width = 115;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(19, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(192, 20);
            this.label2.TabIndex = 7;
            this.label2.Text = "Panel locations and settings";
            // 
            // buttonRestart
            // 
            this.buttonRestart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(158)))), ((int)(((byte)(218)))));
            this.buttonRestart.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonRestart.ForeColor = System.Drawing.Color.White;
            this.buttonRestart.Location = new System.Drawing.Point(732, 354);
            this.buttonRestart.Name = "buttonRestart";
            this.buttonRestart.Size = new System.Drawing.Size(107, 35);
            this.buttonRestart.TabIndex = 19;
            this.buttonRestart.Text = "Restart";
            this.buttonRestart.UseVisualStyleBackColor = false;
            this.buttonRestart.Click += new System.EventHandler(this.buttonRestart_Click);
            // 
            // checkBoxAlwaysOnTop
            // 
            this.checkBoxAlwaysOnTop.AutoSize = true;
            this.checkBoxAlwaysOnTop.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.checkBoxAlwaysOnTop.ForeColor = System.Drawing.Color.White;
            this.checkBoxAlwaysOnTop.Location = new System.Drawing.Point(20, 283);
            this.checkBoxAlwaysOnTop.Name = "checkBoxAlwaysOnTop";
            this.checkBoxAlwaysOnTop.Size = new System.Drawing.Size(159, 24);
            this.checkBoxAlwaysOnTop.TabIndex = 20;
            this.checkBoxAlwaysOnTop.Text = "Panel always on top";
            this.checkBoxAlwaysOnTop.UseVisualStyleBackColor = true;
            this.checkBoxAlwaysOnTop.CheckedChanged += new System.EventHandler(this.checkBoxAlwaysOnTop_CheckedChanged);
            // 
            // checkBoxHidePanelTitleBar
            // 
            this.checkBoxHidePanelTitleBar.AutoSize = true;
            this.checkBoxHidePanelTitleBar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.checkBoxHidePanelTitleBar.ForeColor = System.Drawing.Color.White;
            this.checkBoxHidePanelTitleBar.Location = new System.Drawing.Point(207, 283);
            this.checkBoxHidePanelTitleBar.Name = "checkBoxHidePanelTitleBar";
            this.checkBoxHidePanelTitleBar.Size = new System.Drawing.Size(294, 24);
            this.checkBoxHidePanelTitleBar.TabIndex = 21;
            this.checkBoxHidePanelTitleBar.Text = "Hide panel title bar (Custom panel only)";
            this.checkBoxHidePanelTitleBar.UseVisualStyleBackColor = true;
            this.checkBoxHidePanelTitleBar.CheckedChanged += new System.EventHandler(this.checkBoxHidePanelTitleBar_CheckedChanged);
            // 
            // buttonApplySettings
            // 
            this.buttonApplySettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(158)))), ((int)(((byte)(218)))));
            this.buttonApplySettings.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonApplySettings.ForeColor = System.Drawing.Color.White;
            this.buttonApplySettings.Location = new System.Drawing.Point(19, 354);
            this.buttonApplySettings.Name = "buttonApplySettings";
            this.buttonApplySettings.Size = new System.Drawing.Size(145, 35);
            this.buttonApplySettings.TabIndex = 22;
            this.buttonApplySettings.Text = "Apply Settings";
            this.buttonApplySettings.UseVisualStyleBackColor = false;
            this.buttonApplySettings.Click += new System.EventHandler(this.buttonApplySettings_Click);
            // 
            // buttonSaveSettings
            // 
            this.buttonSaveSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(158)))), ((int)(((byte)(218)))));
            this.buttonSaveSettings.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonSaveSettings.ForeColor = System.Drawing.Color.White;
            this.buttonSaveSettings.Location = new System.Drawing.Point(207, 354);
            this.buttonSaveSettings.Name = "buttonSaveSettings";
            this.buttonSaveSettings.Size = new System.Drawing.Size(145, 35);
            this.buttonSaveSettings.TabIndex = 23;
            this.buttonSaveSettings.Text = "Save Settings";
            this.buttonSaveSettings.UseVisualStyleBackColor = false;
            this.buttonSaveSettings.Click += new System.EventHandler(this.buttonSaveSettings_Click);
            // 
            // UserControlApplySettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.buttonSaveSettings);
            this.Controls.Add(this.buttonApplySettings);
            this.Controls.Add(this.checkBoxHidePanelTitleBar);
            this.Controls.Add(this.checkBoxAlwaysOnTop);
            this.Controls.Add(this.buttonRestart);
            this.Controls.Add(this.panel1);
            this.Name = "UserControlApplySettings";
            this.Size = new System.Drawing.Size(860, 405);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPanels)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonRestart;
        private System.Windows.Forms.DataGridView dataGridViewPanels;
        private System.Windows.Forms.CheckBox checkBoxAlwaysOnTop;
        private System.Windows.Forms.CheckBox checkBoxHidePanelTitleBar;
        private System.Windows.Forms.Button buttonApplySettings;
        private System.Windows.Forms.Button buttonSaveSettings;
        private System.Windows.Forms.DataGridViewTextBoxColumn PanelName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Left;
        private System.Windows.Forms.DataGridViewTextBoxColumn Top;
        private System.Windows.Forms.DataGridViewTextBoxColumn Width;
        private System.Windows.Forms.DataGridViewTextBoxColumn Height;
    }
}
