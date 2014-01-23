using System;
using System.Diagnostics;
using System.Windows.Data;
namespace CITHEPLib.Converters
{
    public class DivideConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double x = System.Convert.ToDouble(values[0]);
            double y = System.Convert.ToDouble(values[1]);
            return x/y;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
