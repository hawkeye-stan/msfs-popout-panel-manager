using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.DynamicLod
{
    public class LodConfig : ObservableObject
    {
        public int Agl { get; set; }

        public int Lod { get; set; }
    }
}
