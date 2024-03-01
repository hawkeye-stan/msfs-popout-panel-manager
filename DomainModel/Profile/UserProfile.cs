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
            this.PropertyChanged += (_, e) =>
            {
                if (e is PropertyChangedExtendedEventArgs { DisableSave: false })
                    OnProfileChanged?.Invoke(this, EventArgs.Empty);

                if (e.PropertyName == nameof(IsUsedLegacyCameraSystem))
                    OnPanelConfigChanged();
            };

            PanelConfigs.CollectionChanged += (_, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        if (e.NewItems?[0] == null)
                            return;

                        ((PanelConfig)e.NewItems[0]).PropertyChanged += (_, arg) =>
                        {
                            if (arg is PropertyChangedExtendedEventArgs { DisableSave: false })
                                OnProfileChanged?.Invoke(this, EventArgs.Empty);

                            if (arg is PropertyChangedExtendedEventArgs changedArg)
                            {
                                if (changedArg.ObjectName == QualifyFullName.Of(nameof(MSFSPopoutPanelManager.DomainModel.Profile.FloatingPanel)) &&
                                    changedArg.PropertyName == nameof(FloatingPanel.IsEnabled))
                                {
                                    if(PanelConfigs.Any(x => x.FloatingPanel.IsEnabled))
                                        OnUseFloatingPanelChanged?.Invoke(this, true);
                                    else
                                        OnUseFloatingPanelChanged?.Invoke(this, false);
                                }
                            }

                            OnPanelConfigChanged();
                        };
                        OnProfileChanged?.Invoke(this, EventArgs.Empty);
                        OnPanelConfigChanged();
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                        OnProfileChanged?.Invoke(this, EventArgs.Empty);
                        OnPanelConfigChanged();
                        break;
                }
            };

            InitializeChildPropertyChangeBinding();
        }

        public event EventHandler<bool> OnUseFloatingPanelChanged;
        public event EventHandler OnProfileChanged;

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public bool IsLocked { get; set; } = false;

        public ObservableCollection<string> AircraftBindings { get; set; } = new();

        public ObservableCollection<PanelConfig> PanelConfigs { get; set; } = new();

        public ProfileSetting ProfileSetting { get; set; } = new();

        public MsfsGameWindowConfig MsfsGameWindowConfig { get; set; } = new();

        public int PanelSourceCockpitZoomFactor { get; set; } = 50;

        public bool IsUsedLegacyCameraSystem { get; set; } = true;

        public int CompareTo(UserProfile other)
        {
            return string.Compare(Name.ToLower(), other.Name.ToLower(), StringComparison.Ordinal);
        }

        [JsonIgnore]
        public bool IsActive { get; set; }

        [JsonIgnore]
        public bool IsEditingPanelSource { get; set; }

        [JsonIgnore]
        public bool IsSelectingPanelSource { get; set; }

        private bool _isPoppedOut;
        [JsonIgnore]
        public bool IsPoppedOut
        {
            get => _isPoppedOut;
            set
            {
                _isPoppedOut = value;

                if (value) 
                    return;

                foreach (var panelConfig in PanelConfigs)
                    panelConfig.PanelHandle = IntPtr.MaxValue;   // reset panel is popped out status
            }
        }

        [JsonIgnore]
        public Guid CurrentMoveResizePanelId { get; set; }

        [JsonIgnore]
        public bool HasUnidentifiedPanelSource { get; private set; }

        [JsonIgnore]
        public bool IsDisabledStartPopOut { get; set; }

        [JsonIgnore]
        public bool HasCustomPanels => PanelConfigs != null && PanelConfigs.Count(p => p.PanelType == PanelType.CustomPopout) > 0;

        private void OnPanelConfigChanged()
        {
            HasUnidentifiedPanelSource = PanelConfigs.Any(p => p.PanelType == PanelType.CustomPopout && p.PanelSource.X == null);
        }
    }
}
