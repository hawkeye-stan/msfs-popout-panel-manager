using MSFSPopoutPanelManager.Shared;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class RefocusOnDisplay : ObservableObject
    {
        public RefocusOnDisplay()
        {
            Monitors = new ObservableCollection<MonitorInfo>();

            Monitors.CollectionChanged += (sender, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        if (e.NewItems?[0] == null)
                            return;

                        ((MonitorInfo)e.NewItems[0]).PropertyChanged += (innerSender, innerArg) =>
                        {
                            if (innerArg is PropertyChangedExtendedEventArgs { DisableSave: false })
                                base.OnPropertyChanged(innerSender, innerArg);
                        };
                        base.OnPropertyChanged(sender, new PropertyChangedEventArgs("Monitors"));
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        base.OnPropertyChanged(sender, new PropertyChangedEventArgs("Monitors"));
                        break;
                }
            };

            InitializeChildPropertyChangeBinding();
        }

        public bool IsEnabled { get; set; } = false;


        public ObservableCollection<MonitorInfo> Monitors { get; set; }
    }
}
