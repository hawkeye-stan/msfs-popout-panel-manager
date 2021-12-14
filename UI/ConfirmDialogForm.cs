using DarkUI.Forms;
using System;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager.UI
{
    public partial class ConfirmDialogForm : DarkForm
    {
        public ConfirmDialogForm(string title, string Message, bool cancellable = true)
        {
            InitializeComponent();
            Text = title;
            labelMessage.Text = Message;

            buttonYes.Visible = cancellable;
            buttonNo.Visible = cancellable;
            buttonOK.Visible = !cancellable;
        }

        private void buttonYes_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonNo_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}