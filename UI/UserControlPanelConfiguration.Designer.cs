
namespace MSFSPopoutPanelManager.UI
{
    partial class UserControlPanelConfiguration
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridViewPanels = new System.Windows.Forms.DataGridView();
            this.PanelName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Left = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Top = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Width = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Height = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AlwaysOnTop = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.HideTitlebar = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonRestart = new System.Windows.Forms.Button();
            this.buttonSaveSettings = new System.Windows.Forms.Button();
            this.buttonPixelMinusLarge = new System.Windows.Forms.Button();
            this.buttonPixelMinusSmall = new System.Windows.Forms.Button();
            this.buttonPixelPlusSmall = new System.Windows.Forms.Button();
            this.buttonPixelPlusLarge = new System.Windows.Forms.Button();
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
            this.panel1.Size = new System.Drawing.Size(915, 348);
            this.panel1.TabIndex = 1;
            // 
            // dataGridViewPanels
            // 
            this.dataGridViewPanels.AllowUserToAddRows = false;
            this.dataGridViewPanels.AllowUserToDeleteRows = false;
            this.dataGridViewPanels.AllowUserToResizeColumns = false;
            this.dataGridViewPanels.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding(3);
            this.dataGridViewPanels.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewPanels.CausesValidation = false;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(10, 3, 3, 3);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewPanels.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewPanels.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPanels.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PanelName,
            this.Left,
            this.Top,
            this.Width,
            this.Height,
            this.AlwaysOnTop,
            this.HideTitlebar});
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle8.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewPanels.DefaultCellStyle = dataGridViewCellStyle8;
            this.dataGridViewPanels.Location = new System.Drawing.Point(20, 35);
            this.dataGridViewPanels.MultiSelect = false;
            this.dataGridViewPanels.Name = "dataGridViewPanels";
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewPanels.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.dataGridViewPanels.RowHeadersVisible = false;
            dataGridViewCellStyle10.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle10.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle10.Padding = new System.Windows.Forms.Padding(3);
            this.dataGridViewPanels.RowsDefaultCellStyle = dataGridViewCellStyle10;
            this.dataGridViewPanels.RowTemplate.Height = 25;
            this.dataGridViewPanels.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridViewPanels.ShowCellErrors = false;
            this.dataGridViewPanels.ShowCellToolTips = false;
            this.dataGridViewPanels.ShowEditingIcon = false;
            this.dataGridViewPanels.ShowRowErrors = false;
            this.dataGridViewPanels.Size = new System.Drawing.Size(874, 310);
            this.dataGridViewPanels.TabIndex = 8;
            // 
            // PanelName
            // 
            this.PanelName.DataPropertyName = "PanelName";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.PanelName.DefaultCellStyle = dataGridViewCellStyle3;
            this.PanelName.FillWeight = 80F;
            this.PanelName.HeaderText = "Panel Name";
            this.PanelName.Name = "PanelName";
            this.PanelName.Width = 300;
            // 
            // Left
            // 
            this.Left.DataPropertyName = "Left";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Left.DefaultCellStyle = dataGridViewCellStyle4;
            this.Left.FillWeight = 80F;
            this.Left.HeaderText = "X Pos";
            this.Left.MaxInputLength = 6;
            this.Left.Name = "Left";
            this.Left.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Left.Width = 95;
            // 
            // Top
            // 
            this.Top.DataPropertyName = "Top";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Top.DefaultCellStyle = dataGridViewCellStyle5;
            this.Top.FillWeight = 80F;
            this.Top.HeaderText = "Y Pos";
            this.Top.MaxInputLength = 6;
            this.Top.Name = "Top";
            this.Top.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Top.Width = 95;
            // 
            // Width
            // 
            this.Width.DataPropertyName = "Width";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Width.DefaultCellStyle = dataGridViewCellStyle6;
            this.Width.FillWeight = 80F;
            this.Width.HeaderText = "Width";
            this.Width.MaxInputLength = 6;
            this.Width.Name = "Width";
            this.Width.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Width.Width = 95;
            // 
            // Height
            // 
            this.Height.DataPropertyName = "Height";
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Height.DefaultCellStyle = dataGridViewCellStyle7;
            this.Height.FillWeight = 80F;
            this.Height.HeaderText = "Height";
            this.Height.MaxInputLength = 6;
            this.Height.Name = "Height";
            this.Height.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Height.Width = 95;
            // 
            // AlwaysOnTop
            // 
            this.AlwaysOnTop.DataPropertyName = "AlwaysOnTop";
            this.AlwaysOnTop.FalseValue = "false";
            this.AlwaysOnTop.FillWeight = 80F;
            this.AlwaysOnTop.HeaderText = "Always on Top";
            this.AlwaysOnTop.Name = "AlwaysOnTop";
            this.AlwaysOnTop.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.AlwaysOnTop.TrueValue = "true";
            this.AlwaysOnTop.Width = 95;
            // 
            // HideTitlebar
            // 
            this.HideTitlebar.DataPropertyName = "HideTitlebar";
            this.HideTitlebar.FalseValue = "false";
            this.HideTitlebar.FillWeight = 80F;
            this.HideTitlebar.HeaderText = "Hide Titlebar";
            this.HideTitlebar.Name = "HideTitlebar";
            this.HideTitlebar.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.HideTitlebar.TrueValue = "true";
            this.HideTitlebar.Width = 95;
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
            this.buttonRestart.Location = new System.Drawing.Point(787, 354);
            this.buttonRestart.Name = "buttonRestart";
            this.buttonRestart.Size = new System.Drawing.Size(107, 35);
            this.buttonRestart.TabIndex = 19;
            this.buttonRestart.Text = "Restart";
            this.buttonRestart.UseVisualStyleBackColor = false;
            // 
            // buttonSaveSettings
            // 
            this.buttonSaveSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(158)))), ((int)(((byte)(218)))));
            this.buttonSaveSettings.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonSaveSettings.ForeColor = System.Drawing.Color.White;
            this.buttonSaveSettings.Location = new System.Drawing.Point(624, 354);
            this.buttonSaveSettings.Name = "buttonSaveSettings";
            this.buttonSaveSettings.Size = new System.Drawing.Size(145, 35);
            this.buttonSaveSettings.TabIndex = 23;
            this.buttonSaveSettings.Text = "Save Profile";
            this.buttonSaveSettings.UseVisualStyleBackColor = false;
            // 
            // buttonPixelMinusLarge
            // 
            this.buttonPixelMinusLarge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(158)))), ((int)(((byte)(218)))));
            this.buttonPixelMinusLarge.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonPixelMinusLarge.ForeColor = System.Drawing.Color.White;
            this.buttonPixelMinusLarge.Location = new System.Drawing.Point(20, 354);
            this.buttonPixelMinusLarge.Name = "buttonPixelMinusLarge";
            this.buttonPixelMinusLarge.Size = new System.Drawing.Size(69, 35);
            this.buttonPixelMinusLarge.TabIndex = 24;
            this.buttonPixelMinusLarge.Text = "-10 px";
            this.buttonPixelMinusLarge.UseVisualStyleBackColor = false;
            // 
            // buttonPixelMinusSmall
            // 
            this.buttonPixelMinusSmall.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(158)))), ((int)(((byte)(218)))));
            this.buttonPixelMinusSmall.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonPixelMinusSmall.ForeColor = System.Drawing.Color.White;
            this.buttonPixelMinusSmall.Location = new System.Drawing.Point(95, 354);
            this.buttonPixelMinusSmall.Name = "buttonPixelMinusSmall";
            this.buttonPixelMinusSmall.Size = new System.Drawing.Size(69, 35);
            this.buttonPixelMinusSmall.TabIndex = 25;
            this.buttonPixelMinusSmall.Text = "-1 px";
            this.buttonPixelMinusSmall.UseVisualStyleBackColor = false;
            // 
            // buttonPixelPlusSmall
            // 
            this.buttonPixelPlusSmall.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(158)))), ((int)(((byte)(218)))));
            this.buttonPixelPlusSmall.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonPixelPlusSmall.ForeColor = System.Drawing.Color.White;
            this.buttonPixelPlusSmall.Location = new System.Drawing.Point(170, 354);
            this.buttonPixelPlusSmall.Name = "buttonPixelPlusSmall";
            this.buttonPixelPlusSmall.Size = new System.Drawing.Size(69, 35);
            this.buttonPixelPlusSmall.TabIndex = 26;
            this.buttonPixelPlusSmall.Text = "+1 px";
            this.buttonPixelPlusSmall.UseVisualStyleBackColor = false;
            // 
            // buttonPixelPlusLarge
            // 
            this.buttonPixelPlusLarge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(158)))), ((int)(((byte)(218)))));
            this.buttonPixelPlusLarge.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonPixelPlusLarge.ForeColor = System.Drawing.Color.White;
            this.buttonPixelPlusLarge.Location = new System.Drawing.Point(245, 354);
            this.buttonPixelPlusLarge.Name = "buttonPixelPlusLarge";
            this.buttonPixelPlusLarge.Size = new System.Drawing.Size(69, 35);
            this.buttonPixelPlusLarge.TabIndex = 27;
            this.buttonPixelPlusLarge.Text = "+10 px";
            this.buttonPixelPlusLarge.UseVisualStyleBackColor = false;
            // 
            // UserControlPanelConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.buttonPixelPlusLarge);
            this.Controls.Add(this.buttonPixelPlusSmall);
            this.Controls.Add(this.buttonPixelMinusSmall);
            this.Controls.Add(this.buttonPixelMinusLarge);
            this.Controls.Add(this.buttonSaveSettings);
            this.Controls.Add(this.buttonRestart);
            this.Controls.Add(this.panel1);
            this.Name = "UserControlPanelConfiguration";
            this.Size = new System.Drawing.Size(915, 405);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPanels)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonRestart;
        private System.Windows.Forms.DataGridView dataGridViewPanels;
        private System.Windows.Forms.Button buttonSaveSettings;
        private System.Windows.Forms.Button buttonPixelMinusLarge;
        private System.Windows.Forms.Button buttonPixelMinusSmall;
        private System.Windows.Forms.Button buttonPixelPlusSmall;
        private System.Windows.Forms.Button buttonPixelPlusLarge;
        private System.Windows.Forms.DataGridViewTextBoxColumn PanelName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Left;
        private System.Windows.Forms.DataGridViewTextBoxColumn Top;
        private System.Windows.Forms.DataGridViewTextBoxColumn Width;
        private System.Windows.Forms.DataGridViewTextBoxColumn Height;
        private System.Windows.Forms.DataGridViewCheckBoxColumn AlwaysOnTop;
        private System.Windows.Forms.DataGridViewCheckBoxColumn HideTitlebar;
    }
}
