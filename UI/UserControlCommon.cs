using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager
{
    public class UserControlCommon : UserControl
    {
        protected PanelManager PanelManager { get; set; }

        public UserControlCommon() { }

        public UserControlCommon(PanelManager panelManager)
        {
            PanelManager = panelManager;
        }
    }
}
