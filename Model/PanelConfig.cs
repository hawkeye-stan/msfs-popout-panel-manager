using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;

namespace MSFSPopoutPanelManager.Model
{
    public class PanelConfig : ObservableObject
    {
        public int PanelIndex { get; set; }

        public string PanelName { get; set; }

        public PanelType PanelType { get; set; }

        public int Top { get; set; }

        public int Left { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool AlwaysOnTop { get; set; }

        public bool HideTitlebar { get; set; }

        public bool FullScreen { get; set; }

        public bool TouchEnabled { get; set; }

        [JsonIgnore]
        public bool IsCustomPopout { get { return PanelType == PanelType.CustomPopout; } }

        [JsonIgnore]
        public IntPtr PanelHandle { get; set; }

        [JsonIgnore]
        public bool IsLockable
        {
            get
            {
                switch (PanelType)
                {
                    case PanelType.CustomPopout:
                    case PanelType.BuiltInPopout:
                    case PanelType.MSFSTouchPanel:
                        return true;
                    default:
                        return false;
                }
            }
        }

        [JsonIgnore]
        public bool HasTouchableEvent
        {
            get
            {
                switch (PanelType)
                {
                    case PanelType.CustomPopout:
                    case PanelType.BuiltInPopout:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}
