using System.Collections.Generic;
using System.Collections.Specialized;

namespace MSFSPopoutPanelManager.DomainModel.DynamicLod
{
    public class ObservableLodConfigLinkedList : LinkedList<LodConfig>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public void OnNotifyCollectionChanged(NotifyCollectionChangedAction action, LodConfig item)
        {
            if (CollectionChanged == null)
                return;

            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
            }
        }

        public new void AddFirst(LodConfig item)
        {
            base.AddFirst(item);
            OnNotifyCollectionChanged(NotifyCollectionChangedAction.Add, item);
        }

        public new void AddLast(LodConfig item)
        {
            base.AddLast(item);
            OnNotifyCollectionChanged(NotifyCollectionChangedAction.Add, item);
        }

        public new void AddBefore(LinkedListNode<LodConfig> node, LinkedListNode<LodConfig> newNode)
        {
            base.AddBefore(node, newNode);
            OnNotifyCollectionChanged(NotifyCollectionChangedAction.Add, newNode.Value);
        }

        public new void AddAfter(LinkedListNode<LodConfig> node, LinkedListNode<LodConfig> newNode)
        {
            base.AddAfter(node, newNode);
            OnNotifyCollectionChanged(NotifyCollectionChangedAction.Add, newNode.Value);
        }

        public new void Remove(LodConfig item)
        {
            base.Remove(item);
            OnNotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item);
        }

        public new void RemoveFirst()
        {
            if (First == null)
                return;

            base.RemoveFirst();
            OnNotifyCollectionChanged(NotifyCollectionChangedAction.Remove, First.Value);
        }

        public new void RemoveLast()
        {
            if (Last == null)
                return;

            base.RemoveLast();
            OnNotifyCollectionChanged(NotifyCollectionChangedAction.Remove, Last.Value);
        }
    }
}
