using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using System;
using System.Linq;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class PanelCoorOverlayViewModel : BaseViewModel
    {
        public PanelCoorOverlayViewModel(SharedStorage sharedStorage) : base(sharedStorage)
        {
        }

        public PanelConfig Panel { get; private set; }

        public void SetPanelId(Guid id)
        {
            Panel = ActiveProfile.PanelConfigs.FirstOrDefault(p => p.Id == id);
        }
    }
}
