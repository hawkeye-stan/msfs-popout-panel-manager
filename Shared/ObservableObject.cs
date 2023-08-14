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
            bool hasJsonIgnoreAttribute = false;

            var type = sender.GetType();
            var propertyInfo = type.GetProperty(e.PropertyName);

            if (propertyInfo != null)
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IgnorePropertyChanged)))
                    return;

                hasJsonIgnoreAttribute = Attribute.IsDefined(propertyInfo, typeof(JsonIgnoreAttribute));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedExtendedEventArgs(e?.PropertyName, sender.ToString(), hasJsonIgnoreAttribute));
        }

        protected void InitializeChildPropertyChangeBinding()
        {
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (PropertyInfo propertyInfo in properties)
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IgnorePropertyChanged)))
                    continue;

                Type childType = propertyInfo.PropertyType;

                if (childType.IsSubclassOf(typeof(ObservableObject)))
                {
                    EventInfo eventInfo = childType.GetEvent("PropertyChanged");
                    MethodInfo methodInfo = type.GetMethod("OnPropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);
                    Delegate dg = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);
                    eventInfo.AddEventHandler(propertyInfo.GetValue(this, null), dg);
                }
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
