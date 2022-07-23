using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class OnlineFeatureOrchestrator : ObservableObject
    {
        public void OpenUserGuide()
        {
            WindowProcessManager.OpenOnlineUserGuide();
        }

        public void OpenLatestDownload()
        {
            WindowProcessManager.OpenOnlineLatestDownload();
        }
    }
}
