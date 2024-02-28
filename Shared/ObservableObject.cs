using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Reflection;

namespace MSFSPopoutPanelManager.Shared
{
    public class ObservableObject : INotifyPropertyChanged
    {
        // Implements Fody.Changed
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var hasJsonIgnoreAttribute = false;

            var type = sender.GetType();
            if (e.PropertyName != null)
            {
                var propertyInfo = type.GetProperty(e.PropertyName);

                if (propertyInfo != null)
                {
                    if (Attribute.IsDefined(propertyInfo, typeof(IgnorePropertyChanged)))
                        return;

                    hasJsonIgnoreAttribute = Attribute.IsDefined(propertyInfo, typeof(JsonIgnoreAttribute));
                }
            }

            PropertyChanged?.Invoke(this, new PropertyChangedExtendedEventArgs(e.PropertyName, sender.ToString(), hasJsonIgnoreAttribute));
        }

        protected void InitializeChildPropertyChangeBinding()
        {
            var type = this.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var propertyInfo in properties)
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IgnorePropertyChanged)))
                    continue;

                var childType = propertyInfo.PropertyType;

                if (!childType.IsSubclassOf(typeof(ObservableObject))) 
                    continue;

                var eventInfo = childType.GetEvent("PropertyChanged");
                var methodInfo = type.GetMethod("OnPropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);

                if (eventInfo == null || eventInfo.EventHandlerType == null || methodInfo == null)
                    continue;

                var dg = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);

                if(propertyInfo.GetValue(this, null) != null)
                    eventInfo.AddEventHandler(propertyInfo.GetValue(this, null), dg);
            }
        }
    }

    public class PropertyChangedExtendedEventArgs : PropertyChangedEventArgs
    {
        public virtual bool DisableSave { get; private set; }

        public virtual string ObjectName { get; private set; }

        public PropertyChangedExtendedEventArgs(string propertyName, string objectName, bool disableSave) : base(propertyName)
        {
            DisableSave = disableSave;
            ObjectName = objectName;
        }
    }
}
