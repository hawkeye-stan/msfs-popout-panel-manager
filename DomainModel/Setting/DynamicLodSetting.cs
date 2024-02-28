using MSFSPopoutPanelManager.DomainModel.DynamicLod;
using MSFSPopoutPanelManager.Shared;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class DynamicLodSetting : ObservableObject
    {
        public DynamicLodSetting()
        {
            InitializeChildPropertyChangeBinding();

            TlodConfigs.CollectionChanged += (_, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    if (e.NewItems?[0] is LodConfig item)
                        item.PropertyChanged += (_, _) => TlodConfigs.OnNotifyCollectionChanged(NotifyCollectionChangedAction.Reset, item);
                }
            };

            OlodConfigs.CollectionChanged += (_, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    if (e.NewItems?[0] is LodConfig item)
                        item.PropertyChanged += (_, _) => OlodConfigs.OnNotifyCollectionChanged(NotifyCollectionChangedAction.Reset, item);
                }
            };
        }

        public bool IsEnabled { get; set; } = false;

        public ObservableLodConfigLinkedList TlodConfigs { get; set; } = new();

        public ObservableLodConfigLinkedList OlodConfigs { get; set; } = new();

        public bool ResetEnabled { get; set; } = false;

        public int ResetTlod { get; set; } = 100;

        public int ResetOlod { get; set; } = 100;

        public void AddDefaultTLodConfigs()
        {
            TlodConfigs.AddLast(new LinkedListNode<LodConfig>(new LodConfig { Agl = 0, Lod = 100 }));
            TlodConfigs.AddLast(new LinkedListNode<LodConfig>(new LodConfig { Agl = 5000, Lod = 200 }));
            TlodConfigs.AddLast(new LinkedListNode<LodConfig>(new LodConfig { Agl = 10000, Lod = 300 }));
            TlodConfigs.AddLast(new LinkedListNode<LodConfig>(new LodConfig { Agl = 20000, Lod = 400 }));
        }

        public void AddDefaultOLodConfigs()
        {
            OlodConfigs.AddLast(new LinkedListNode<LodConfig>(new LodConfig { Agl = 0, Lod = 200 }));
            OlodConfigs.AddLast(new LinkedListNode<LodConfig>(new LodConfig { Agl = 1000, Lod = 150 }));
            OlodConfigs.AddLast(new LinkedListNode<LodConfig>(new LodConfig { Agl = 5000, Lod = 100 }));
        }

        [OnDeserialized]
        private void OnDeserialization(StreamingContext context)
        {
            foreach (var item in TlodConfigs)
                item.PropertyChanged += (_, _) => TlodConfigs.OnNotifyCollectionChanged(NotifyCollectionChangedAction.Reset, item);

            foreach (var item in OlodConfigs)
                item.PropertyChanged += (_, _) => OlodConfigs.OnNotifyCollectionChanged(NotifyCollectionChangedAction.Reset, item);
        }
    }
}
