using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;

namespace MSFSPopoutPanelManager.UserDataAgent
{
    public class Profile : ObservableObject
    {
        public Profile()
        {
            BindingAircrafts = new ObservableCollection<string>();
            PanelConfigs = new ObservableCollection<PanelConfig>();
            PanelSourceCoordinates = new ObservableCollection<PanelSourceCoordinate>();
            TouchPanelBindings = new ObservableCollection<TouchPanelBinding>();
            IsLocked = false;
        }

        public int ProfileId { get; set; }

        public string ProfileName { get; set; }

        [JsonConverter(typeof(SingleValueArrayConvertor<string>))]
        public ObservableCollection<string> BindingAircrafts { get; set; }

        public ObservableCollection<PanelSourceCoordinate> PanelSourceCoordinates { get; set; }

        public ObservableCollection<PanelConfig> PanelConfigs { get; set; }

        public ObservableCollection<TouchPanelBinding> TouchPanelBindings { get; set; }

        public bool IsLocked { get; set; }

        public bool PowerOnRequiredForColdStart { get; set; }

        [JsonIgnore]
        public bool IsActive { get; set; }

        [JsonIgnore]
        public bool HasBindingAircrafts
        {
            get
            {
                if (BindingAircrafts == null)
                    return false;

                return BindingAircrafts.Count > 0;
            }
        }

        public void Reset()
        {
            PanelSourceCoordinates.Clear();
            PanelConfigs.Clear();
            IsLocked = false;
        }
    }

    public class PanelSourceCoordinate : ObservableObject
    {
        public int PanelIndex { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        [JsonIgnore]
        public IntPtr PanelHandle { get; set; }
    }

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

        public bool DisableGameRefocus { get; set; }

        [JsonIgnore]
        public bool IsCustomPopOut { get { return PanelType == PanelType.CustomPopout; } }

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

    public class TouchPanelBinding : ObservableObject
    {
        public string PlaneId { get; set; }

        public string PanelId { get; set; }
    }
}
