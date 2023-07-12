using System.Globalization;
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace MSFSPopoutPanelManager.MainApp
{
    public partial class PopOutPanelList : UserControl
    {
        public PopOutPanelList()
        {
            InitializeComponent();
        }
    }

    public class DummyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
