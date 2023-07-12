using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class PanelConfig : ObservableObject
    {
        public PanelConfig()
        {
            if (Id == Guid.Empty)
                Id = Guid.NewGuid();

            PanelName = "Default Panel Name";
            PanelHandle = IntPtr.MaxValue;
            AutoGameRefocus = true;
            PanelSource = new PanelSource();

            InitializeChildPropertyChangeBinding();

            PropertyChanged += PanelConfig_PropertyChanged;
        }

        private void PanelConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FullScreen" && FullScreen)
            {
                AlwaysOnTop = false;
                HideTitlebar = false;
            }
            else if (e.PropertyName == "TouchEnabled" && TouchEnabled)
            {
                AutoGameRefocus = true;
            }
        }

        public Guid Id { get; set; }

        public PanelType PanelType { get; set; }

        public string PanelName { get; set; }

        public int Top { get; set; }

        public int Left { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool AlwaysOnTop { get; set; }

        public bool HideTitlebar { get; set; }

        public bool FullScreen { get; set; }

        public bool TouchEnabled { get; set; }

        public bool AutoGameRefocus { get; set; }

        public PanelSource PanelSource { get; set; }

        [JsonIgnore]
        public IntPtr PanelHandle { get; set; }

        [JsonIgnore]
        public bool IsEditingPanel { get; set; }

        [JsonIgnore]
        public bool IsCustomPopOut => PanelType == PanelType.CustomPopout;

        [JsonIgnore]
        public bool IsBuiltInPopOut => PanelType == PanelType.BuiltInPopout;

        [JsonIgnore]
        public bool HasPanelSource => PanelType == PanelType.BuiltInPopout || (PanelType == PanelType.CustomPopout && PanelSource != null && PanelSource.X != null);

        [JsonIgnore]
        public bool? IsPopOutSuccess
        {
            get
            {
                if (PanelHandle == IntPtr.MaxValue)
                    return null;

                if (PanelHandle == IntPtr.Zero)
                    return false;

                return true;
            }
        }

        [JsonIgnore]
        public bool IsSelectedPanelSource { get; set; }

        [JsonIgnore]
        public bool IsShownPanelSource { get; set; }
    }
}
