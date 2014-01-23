using System.Windows.Data;
using System;

namespace CITHEPLib.Converters
{
    public class InverseConverter: IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool tmp = System.Convert.ToBoolean(value);
            return !tmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool tmp = System.Convert.ToBoolean(value);
            return !tmp;
        }
    }
}
