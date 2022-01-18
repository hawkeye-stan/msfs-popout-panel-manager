
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
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
            this.buttonPixelLargeMinus = new System.Windows.Forms.Button();
            this.buttonPixelMinusSmall = new System.Windows.Forms.Button();
            this.buttonPixelPlusSmall = new System.Windows.Forms.Button();
            this.buttonPixelPlusLarge = new System.Windows.Forms.Button();
            this.buttonLockPanel = new System.Windows.Forms.Button();
            this.toolTipLargeMinus = new System.Windows.Forms.ToolTip(this.components);
            this.toolTipSmallMinus = new System.Windows.Forms.ToolTip(this.components);
            this.toolTipSmallPlus = new System.Windows.Forms.ToolTip(this.components);
            this.toolTipLargePlus = new System.Windows.Forms.ToolTip(this.components);
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
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle11.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle11.Padding = new System.Windows.Forms.Padding(3);
            this.dataGridViewPanels.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle11;
            this.dataGridViewPanels.CausesValidation = false;
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle12.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle12.Padding = new System.Windows.Forms.Padding(10, 3, 3, 3);
            dataGridViewCellStyle12.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewPanels.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle12;
            this.dataGridViewPanels.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPanels.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PanelName,
            this.Left,
            this.Top,
            this.Width,
            this.Height,
            this.AlwaysOnTop,
            this.HideTitlebar});
            dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle18.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle18.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewPanels.DefaultCellStyle = dataGridViewCellStyle18;
            this.dataGridViewPanels.Location = new System.Drawing.Point(20, 35);
            this.dataGridViewPanels.MultiSelect = false;
            this.dataGridViewPanels.Name = "dataGridViewPanels";
            dataGridViewCellStyle19.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle19.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle19.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle19.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle19.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle19.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewPanels.RowHeadersDefaultCellStyle = dataGridViewCellStyle19;
            this.dataGridViewPanels.RowHeadersVisible = false;
            dataGridViewCellStyle20.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle20.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle20.Padding = new System.Windows.Forms.Padding(3);
            this.dataGridViewPanels.RowsDefaultCellStyle = dataGridViewCellStyle20;
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
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.PanelName.DefaultCellStyle = dataGridViewCellStyle13;
            this.PanelName.FillWeight = 80F;
            this.PanelName.HeaderText = "Panel Name";
            this.PanelName.Name = "PanelName";
            this.PanelName.Width = 300;
            // 
            // Left
            // 
            this.Left.DataPropertyName = "Left";
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Left.DefaultCellStyle = dataGridViewCellStyle14;
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
            dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Top.DefaultCellStyle = dataGridViewCellStyle15;
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
            dataGridViewCellStyle16.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Width.DefaultCellStyle = dataGridViewCellStyle16;
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
            dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Height.DefaultCellStyle = dataGridViewCellStyle17;
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
            // buttonPixelLargeMinus
            // 
            this.buttonPixelLargeMinus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(158)))), ((int)(((byte)(218)))));
            this.buttonPixelLargeMinus.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonPixelLargeMinus.ForeColor = System.Drawing.Color.White;
            this.buttonPixelLargeMinus.Location = new System.Drawing.Point(20, 354);
            this.buttonPixelLargeMinus.Name = "buttonPixelLargeMinus";
            this.buttonPixelLargeMinus.Size = new System.Drawing.Size(69, 35);
            this.buttonPixelLargeMinus.TabIndex = 24;
            this.buttonPixelLargeMinus.Text = "-10 px";
            this.toolTipLargeMinus.SetToolTip(this.buttonPixelLargeMinus, "Ctrl -");
            this.buttonPixelLargeMinus.UseVisualStyleBackColor = false;
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
            this.toolTipSmallMinus.SetToolTip(this.buttonPixelMinusSmall, "Ctrl [");
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
            this.toolTipSmallPlus.SetToolTip(this.buttonPixelPlusSmall, "Ctrl ]");
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
            this.toolTipLargePlus.SetToolTip(this.buttonPixelPlusLarge, "Ctrl +");
            this.buttonPixelPlusLarge.UseVisualStyleBackColor = false;
            // 
            // buttonLockPanel
            // 
            this.buttonLockPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(158)))), ((int)(((byte)(218)))));
            this.buttonLockPanel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonLockPanel.ForeColor = System.Drawing.Color.White;
            this.buttonLockPanel.Location = new System.Drawing.Point(772, 354);
            this.buttonLockPanel.Name = "buttonLockPanel";
            this.buttonLockPanel.Size = new System.Drawing.Size(122, 35);
            this.buttonLockPanel.TabIndex = 28;
            this.buttonLockPanel.Text = "Lock Panels";
            this.buttonLockPanel.UseVisualStyleBackColor = false;
            // 
            // UserControlPanelConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.buttonLockPanel);
            this.Controls.Add(this.buttonPixelPlusLarge);
            this.Controls.Add(this.buttonPixelPlusSmall);
            this.Controls.Add(this.buttonPixelMinusSmall);
            this.Controls.Add(this.buttonPixelLargeMinus);
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
        private System.Windows.Forms.DataGridView dataGridViewPanels;
        private System.Windows.Forms.Button buttonPixelLargeMinus;
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
        private System.Windows.Forms.Button buttonLockPanel;
        private System.Windows.Forms.ToolTip toolTipLargeMinus;
        private System.Windows.Forms.ToolTip toolTipSmallMinus;
        private System.Windows.Forms.ToolTip toolTipSmallPlus;
        private System.Windows.Forms.ToolTip toolTipLargePlus;
    }
}
