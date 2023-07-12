using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class MsfsGameWindowConfig : ObservableObject
    {
        public MsfsGameWindowConfig()
        {
            Top = 0;
            Left = 0;
            Width = 0;
            Height = 0;
        }

        public MsfsGameWindowConfig(int left, int top, int width, int height)
        {
            Top = top;
            Left = left;
            Width = width;
            Height = height;
        }

        public int Top { get; set; }

        public int Left { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        [JsonIgnore]
        public bool IsValid => Width != 0 && Height != 0;
    }
}
