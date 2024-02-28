using System;
using System.Windows.Data;

namespace MSFSPopoutPanelManager.MainApp.Converter
{
    public class InverseBooleanOrConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.LongLength <= 0) 
                return true;

            foreach (var value in values)
            {
                if (value is true)
                {
                    return false;
                }
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return new object[] { false };
        }
    }
}
