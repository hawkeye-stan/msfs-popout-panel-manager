
namespace MSFSPopoutPanelManager
{
    partial class PopoutCoorOverlayForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PopoutCoorOverlayForm));
            this.lblPanelIndex = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblPanelIndex
            // 
            this.lblPanelIndex.BackColor = System.Drawing.Color.Transparent;
            this.lblPanelIndex.CausesValidation = false;
            this.lblPanelIndex.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblPanelIndex.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblPanelIndex.Image = ((System.Drawing.Image)(resources.GetObject("lblPanelIndex.Image")));
            this.lblPanelIndex.Location = new System.Drawing.Point(0, 0);
            this.lblPanelIndex.Margin = new System.Windows.Forms.Padding(0);
            this.lblPanelIndex.Name = "lblPanelIndex";
            this.lblPanelIndex.Size = new System.Drawing.Size(62, 44);
            this.lblPanelIndex.TabIndex = 0;
            this.lblPanelIndex.Text = "1";
            this.lblPanelIndex.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PopoutCoorOverlayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(62, 44);
            this.ControlBox = false;
            this.Controls.Add(this.lblPanelIndex);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PopoutCoorOverlayForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "PopoutCoorOverlay";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.SystemColors.Control;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblPanelIndex;
    }
}