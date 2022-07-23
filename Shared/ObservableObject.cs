using System.ComponentModel;

namespace MSFSPopoutPanelManager.Shared
{
    public class ObservableObject : INotifyPropertyChanged
    {
        // Implements Fody.Changed
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName, object oldvalue, object newvalue)
        {
            if (oldvalue != newvalue)
                PropertyChanged?.Invoke(this, new PropertyChangedExtendedEventArgs(propertyName, oldvalue, newvalue));
        }
    }

    public class PropertyChangedExtendedEventArgs : PropertyChangedEventArgs
    {
        public virtual object OldValue { get; private set; }
        public virtual object NewValue { get; private set; }

        public PropertyChangedExtendedEventArgs(string propertyName, object oldValue, object newValue) : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
