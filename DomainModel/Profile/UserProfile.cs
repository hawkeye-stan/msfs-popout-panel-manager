using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    [SuppressPropertyChangedWarnings]
    public class UserProfile : ObservableObject, IComparable<UserProfile>
    {
        public UserProfile()
        {
            Id = Guid.NewGuid();
            IsLocked = false;

            AircraftBindings = new ObservableCollection<string>();
            PanelConfigs = new ObservableCollection<PanelConfig>();
            ProfileSetting = new ProfileSetting();
            MsfsGameWindowConfig = new MsfsGameWindowConfig();

            this.PropertyChanged += (sender, e) =>
            {
                var evtArg = e as PropertyChangedExtendedEventArgs;
                if (!evtArg.DisableSave)
                    ProfileChanged?.Invoke(this, null);
            };

            PanelConfigs.CollectionChanged += (sender, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        if (e.NewItems[0] == null)
                            return;

                        ((PanelConfig)e.NewItems[0]).PropertyChanged += (sender, e) =>
                        {
                            var evtArg = e as PropertyChangedExtendedEventArgs;
                            if (!evtArg.DisableSave)
                                ProfileChanged?.Invoke(this, null);

                            OnPanelConfigChanged(sender, e);
                        };
                        ProfileChanged?.Invoke(this, null);
                        OnPanelConfigChanged(sender, e);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                        ProfileChanged?.Invoke(this, null);
                        OnPanelConfigChanged(sender, e);
                        break;
                }
            };

            InitializeChildPropertyChangeBinding();
        }

        public event EventHandler ProfileChanged;

        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsLocked { get; set; }

        public ObservableCollection<string> AircraftBindings { get; set; }

        public ObservableCollection<PanelConfig> PanelConfigs { get; set; }

        public ProfileSetting ProfileSetting { get; set; }

        public MsfsGameWindowConfig MsfsGameWindowConfig { get; set; }

        public int CompareTo(UserProfile other)
        {
            int result = this.Name.ToLower().CompareTo(other.Name.ToLower());
            if (result == 0)
                result = this.Name.ToLower().CompareTo(other.Name.ToLower());
            return result;
        }

        [JsonIgnore]
        public bool IsActive { get; set; }

        [JsonIgnore]
        public bool IsEditingPanelSource { get; set; }

        private bool _isPoppedOut;
        [JsonIgnore]
        public bool IsPoppedOut
        {
            get { return _isPoppedOut; }
            set
            {
                _isPoppedOut = value;

                if (!value)
                {
                    foreach (var panelConfig in PanelConfigs)
                        panelConfig.PanelHandle = IntPtr.MaxValue;   // reset panel is popped out status
                }
            }
        }

        [JsonIgnore]
        public Guid CurrentMoveResizePanelId { get; set; }

        [JsonIgnore]
        public bool HasUnidentifiedPanelSource { get; private set; }

        [JsonIgnore]
        public bool HasAircraftBindings => AircraftBindings != null && AircraftBindings.Count > 0;

        [JsonIgnore]
        public bool HasCustomPanels => PanelConfigs.Count(p => p.PanelType == PanelType.CustomPopout) > 0;

        private void OnPanelConfigChanged(object sender, EventArgs e)
        {
            HasUnidentifiedPanelSource = PanelConfigs.Any(p => p.PanelType == PanelType.CustomPopout && p.PanelSource.X == null);
        }
    }
}
