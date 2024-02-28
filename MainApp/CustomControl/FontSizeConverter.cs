using System;
using System.Globalization;
using System.Windows.Data;

namespace MSFSPopoutPanelManager.MainApp.CustomControl
{
    public class FontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) 
                return 0.0;

            var v = (double)value;
            return v * 0.6;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
