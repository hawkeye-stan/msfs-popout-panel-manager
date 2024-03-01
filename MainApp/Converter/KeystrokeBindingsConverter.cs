using System;
using System.Linq;
using System.Windows.Data;

namespace MSFSPopoutPanelManager.MainApp.Converter
{
    public class KeystrokeBindingsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(string))
                return "Not Assigned";

            var joinParam = parameter == null ? string.Empty : parameter.ToString();

            var items = value.ToString().Split("|");

            return string.Join(joinParam, items.ToArray());
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
