using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace MSFSPopoutPanelManager.Shared
{
    [SuppressPropertyChangedWarnings]
    public class SortedObservableCollection<T> : ObservableCollection<T> where T : IComparable<T>
    {
        public SortedObservableCollection() { }

        public SortedObservableCollection(IEnumerable<T> objects) : base(objects) { }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            if (e.Action is NotifyCollectionChangedAction.Reset or NotifyCollectionChangedAction.Move or NotifyCollectionChangedAction.Remove) 
                return;

            var query = this.Select((item, index) => (Item: item, Index: index)).OrderBy(tuple => tuple.Item, Comparer.Default);
            var map = query.Select((tuple, index) => (OldIndex: tuple.Index, NewIndex: index)).Where(o => o.OldIndex != o.NewIndex);
            using var enumerator = map.GetEnumerator();
            if (enumerator.MoveNext())
            {
                base.MoveItem(enumerator.Current.OldIndex, enumerator.Current.NewIndex);
            }
        }

        // (optional) user is not allowed to move items in a sorted collection
        protected override void MoveItem(int oldIndex, int newIndex) => throw new InvalidOperationException();
        protected override void SetItem(int index, T item) => throw new InvalidOperationException();

        private class Comparer : IComparer<T>
        {
            public static readonly Comparer Default = new();

            public int Compare(T x, T y) => x.CompareTo(y);
        }

        public virtual void Sort()
        {
            if (Items.Count <= 1)
                return;

            var items = Items.ToList();
            Items.Clear();
            items.Sort();
            foreach (var item in items)
            {
                Items.Add(item);
            }
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public ObservableCollection<string> Join(Func<object, object> value)
        {
            throw new NotImplementedException();
        }
    }
}
